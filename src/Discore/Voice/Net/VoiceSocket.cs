using Discore.WebSocket.Net;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Voice.Net
{
    class VoiceSocket : DiscordClientWebSocket, IDisposable
    {
        public bool IsConnected { get { return State == WebSocketState.Open; } }

        public bool IsPaused { get; set; }

        public int BytesToSend { get { return sendBuffer.Count; } }

        public event EventHandler<Exception> OnError;

        DiscoreGuildCache guildCache;
        DiscoreMemberCache memberCache;
        string endpoint;
        string token;
        int heartbeatInterval;
        VoiceUDPSocketOld udpSocket;
        int ssrc;
        byte[] key;
        bool readyToSendVoice;

        OpusEncoder encoder;
        CircularBuffer sendBuffer;

        const int FRAME_LENGTH = 20;
        const int BITS_PER_SAMPLE = 16;
        const int MAX_OPUS_SIZE = 4000;
        const int BUFFER_LENGTH = 1000;

        int samplesPerFrame, sampleSize, frameSize;
        ushort sequence;

        Task sendTask;
        Task heartbeatTask;
        CancellationTokenSource taskCancellationSource;

        DiscoreLogger log;

        [DllImport("libsodium", EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl)]
        unsafe static extern int SecretBoxEasy(byte* output, byte[] input, long inputLength, byte[] nonce, byte[] secret);

        static unsafe int Encrypt(byte[] input, long inputLength, byte[] output, int outputOffset, byte[] nonce, byte[] secret)
        {
            fixed (byte* outPtr = output)
                return SecretBoxEasy(outPtr + outputOffset, input, inputLength, nonce, secret);
        }

        public VoiceSocket(string loggingName, DiscoreGuildCache guildCache, DiscoreMemberCache memberCache)
            : base(loggingName)
        {
            this.guildCache = guildCache;
            this.memberCache = memberCache;

            log = new DiscoreLogger(loggingName);

            encoder = new OpusEncoder(48000, 2, 20, null, OpusApplication.MusicOrMixed);

            samplesPerFrame = encoder.InputSamplingRate / 1000 * FRAME_LENGTH;
            sampleSize = (BITS_PER_SAMPLE / 8) * encoder.InputChannels;
            frameSize = samplesPerFrame * sampleSize;

            sendBuffer = new CircularBuffer((int)Math.Ceiling(BUFFER_LENGTH / (double)FRAME_LENGTH) * frameSize);

            socket.OnMessageReceived += VoiceSocket_OnMessageReceived;
            socket.OnError += Socket_OnError;
        }

        private async void UdpSocket_OnError(object sender, Exception ex)
        {
            udpSocket.OnError -= UdpSocket_OnError;
            udpSocket.OnIPDiscovered -= UdpSocket_OnIPDiscovered;

            await HandleFatalError(ex).ConfigureAwait(false);
        }

        /// <exception cref="InvalidOperationException">Thrown if this socket is already connected or connecting.</exception>
        public async Task<bool> ConnectAsync(string endpoint, string token)
        {
            if (socket.State != DiscoreWebSocketState.Closed)
                throw new InvalidOperationException("Failed to connect, the socket is already connected or connecting.");

            endpoint = endpoint.Split(':')[0];
            this.endpoint = endpoint;
            this.token = token;

            sequence = 0;
            sendBuffer.Reset();

            string uri = $"wss://{endpoint}";
            log.LogVerbose($"Connecting to voice websocket {uri}...");

            if (await socket.ConnectAsync(uri, CancellationToken.None).ConfigureAwait(false))
            {
                log.LogVerbose($"Connected to voice websocket {uri}.");

                taskCancellationSource = new CancellationTokenSource();

                // Start new tasks
                sendTask = SendLoop();
                heartbeatTask = HeartbeatLoop();

                // Send the identify payload
                await SendIdentifyPayload();
                return true;
            }
            else
                return false;
        }

        /// <exception cref="InvalidOperationException">Thrown if this socket is not connected.</exception>
        /// <exception cref="TaskCanceledException"></exception>
        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if (socket.State != DiscoreWebSocketState.Open)
                throw new InvalidOperationException("Failed to disconnect, the socket is not open.");

            taskCancellationSource?.Cancel();

            // Close WebSocket
            await socket.DisconnectAsync(cancellationToken).ConfigureAwait(false);

            // Close the UDP socket if still open
            if (udpSocket != null)
                udpSocket.Shutdown();

            // Wait for tasks
            await Task.WhenAll(sendTask, heartbeatTask).ConfigureAwait(false);
        }

        /// <exception cref="OperationCanceledException"></exception>
        public void SendPCMData(byte[] data, int offset, int count)
        {
            sendBuffer.Write(data, offset, count);
        }

        public bool CanSendData(int size)
        {
            return sendBuffer.MaxLength - sendBuffer.Count >= size;
        }

        public void ClearVoiceBuffer()
        {
            sendBuffer.Reset();
        }

        async Task StartIPDiscovery()
        {
            log.LogVerbose("[IPDiscovery] Starting ip discovery...");

            await udpSocket.StartIPDiscoveryAsync(ssrc).ConfigureAwait(false);
        }

        private void UdpSocket_OnIPDiscovered(object sender, IPDiscoveryEventArgs e)
        {
            log.LogVerbose($"[IPDiscovery] IP has been discovered. Endpoint: {e.IP}:{e.Port}");
            SendSelectProtocol(e.IP, e.Port);
        }

        async Task SendLoop()
        {
            //const int TICKS_PER_S = 1000;
            const int TICKS_PER_MS = 1;

            try
            {
                // Wait for full connection
                while (!readyToSendVoice || socket.State != DiscoreWebSocketState.Open)
                    await Task.Delay(1000, taskCancellationSource.Token).ConfigureAwait(false);

                byte[] frame = new byte[encoder.FrameSize];
                byte[] encodedFrame = new byte[MAX_OPUS_SIZE];

                uint timestamp = 0;

                int ticksPerFrame = TICKS_PER_MS * encoder.FrameLength;
                uint samplesPerFrame = (uint)encoder.SamplesPerFrame;

                byte[] nonce = new byte[24];
                byte[] voicePacket = new byte[MAX_OPUS_SIZE + 12 + 16];
                byte[] pingPacket = new byte[8];

                int rtpPacketLength = 0;

                // Setup RTP packet header
                voicePacket[0] = 0x80; // Packet Type
                voicePacket[1] = 0x78; // Packet Version
                voicePacket[8] = (byte)(ssrc >> 24); // ssrc
                voicePacket[9] = (byte)(ssrc >> 16);
                voicePacket[10] = (byte)(ssrc >> 8);
                voicePacket[11] = (byte)(ssrc >> 0);

                // Copy RTP packet header into nonce
                Buffer.BlockCopy(voicePacket, 0, nonce, 0, 12);

                int nextTicks = Environment.TickCount;
                int nextPingTicks = Environment.TickCount;

                // Begin send loop
                bool hasFrame = false;
                while (socket.State == DiscoreWebSocketState.Open)
                {
                    // If we don't have a frame to send and we have a full frame buffered
                    if (!hasFrame && sendBuffer.Count > 0)
                    {
                        // Read frame from buffer
                        sendBuffer.Read(frame, 0, frame.Length);

                        // Increase the sequence number, use unchecked because wrapping is valid
                        unchecked { sequence++; };

                        // Set sequence number in RTP packet
                        voicePacket[2] = (byte)(sequence >> 8);
                        voicePacket[3] = (byte)(sequence >> 0);
                        // Set timestamp in RTP packet
                        voicePacket[4] = (byte)(timestamp >> 24);
                        voicePacket[5] = (byte)(timestamp >> 16);
                        voicePacket[6] = (byte)(timestamp >> 8);
                        voicePacket[7] = (byte)(timestamp >> 0);

                        // Encode the frame
                        int encodedLength = encoder.EncodeFrame(frame, 0, encodedFrame);

                        // Update the separately stored nonce from RTP packet
                        Buffer.BlockCopy(voicePacket, 2, nonce, 2, 6);

                        // Encrypt the frame
                        int encryptStatus = Encrypt(encodedFrame, encodedLength, voicePacket, 12, nonce, key);
                        if (encryptStatus == 0)
                        {
                            // Update timestamp
                            timestamp = unchecked(timestamp + samplesPerFrame);

                            rtpPacketLength = encodedLength + 12 + 16;
                            hasFrame = true;
                        }
                        else
                        {
                            // Failed to encrypt
                            log.LogError($"Failed to encrypt RTP packet. encryptStatus: {encryptStatus}");
                        }
                    }

                    int currentTicks = Environment.TickCount;
                    int ticksToNextFrame = nextTicks - currentTicks;
                    // Is it time to send the next frame?
                    if (ticksToNextFrame <= 0)
                    {
                        if (IsPaused)
                        {
                            // If we are paused, do nothing.
                            await Task.Delay(1, taskCancellationSource.Token).ConfigureAwait(false);
                        }
                        // If we have a frame to send
                        else if (hasFrame)
                        {
                            hasFrame = false;
                            // Send the frame across UDP
                            //udpSocket.Send(voicePacket, rtpPacketLength).Wait();
                            await udpSocket.SendAsync(voicePacket, rtpPacketLength).ConfigureAwait(false);
                        }

                        // Calculate the time for next frame
                        nextTicks += ticksPerFrame;

                        // Is it time to ping?
                        //if (currentTicks > nextPingTicks)
                        //{
                        //    // Increment the ping packet
                        //    for (int i = 0; i < 8; i++)
                        //    {
                        //        unchecked
                        //        {
                        //            pingPacket[i] = (byte)(pingPacket[i] + 1);
                        //        }
                        //    }

                        //    // Send the ping packet across UDP
                        //    udpSocket.Send(pingPacket, pingPacket.Length).Wait();
                        //    nextPingTicks = currentTicks + 5 * TICKS_PER_S;
                        //}
                    }
                    else
                    {
                        // Nothing to do, so sleep for a bit to avoid burning cpu cycles
                        await Task.Delay(1, taskCancellationSource.Token).ConfigureAwait(false);
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                await HandleFatalError(ex, sendTask).ConfigureAwait(false);
            }
        }

        async Task HandleFatalError(Exception ex, Task originatingTask = null)
        {
            // Log error
            log.LogError(ex);

            taskCancellationSource?.Cancel();

            // Close WebSocket
            if (socket.State == DiscoreWebSocketState.Open)
                await socket.DisconnectAsync(CancellationToken.None, WebSocketCloseStatus.InternalServerError)
                    .ConfigureAwait(false);

            // Close the UDP socket if still open
            if (udpSocket != null)
                udpSocket.Shutdown();

            // Let all tasks that did not invoke this method end.
            if (originatingTask == heartbeatTask)
                await sendTask.ConfigureAwait(false);
            else if (originatingTask == sendTask)
                await heartbeatTask.ConfigureAwait(false);
            else
                await Task.WhenAll(sendTask, heartbeatTask).ConfigureAwait(false);

            OnError?.Invoke(this, ex);
        }

        public override void Dispose()
        {
            taskCancellationSource?.Dispose();

            if (udpSocket != null)
            {
                udpSocket.OnIPDiscovered -= UdpSocket_OnIPDiscovered;
                udpSocket.Dispose();
            }

            socket.Dispose();
            base.Dispose(); // TODO
        }
    }
}
