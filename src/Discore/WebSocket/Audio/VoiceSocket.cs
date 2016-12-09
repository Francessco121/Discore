using Discore.WebSocket.Net;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace Discore.WebSocket.Audio
{
    class VoiceSocket : IDisposable
    {
        public DiscordGuildMember Member { get { return memberCache.Value; } }
        public bool IsConnected { get { return socket.State == WebSocketState.Open; } }

        public int BytesToSend { get { return sendBuffer.Count; } }

        public event EventHandler<Exception> OnError;

        DiscoreWebSocket socket;
        DiscoreGuildCache guildCache;
        DiscoreMemberCache memberCache;
        string endpoint;
        string token;
        int heartbeatInterval;
        VoiceUDPSocket udpSocket;
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

        Thread sendThread;
        Thread heartbeatThread;

        CancellationTokenSource cancelTokenSource;
        DiscoreLogger log;

        [DllImport("libsodium", EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl)]
        unsafe static extern int SecretBoxEasy(byte* output, byte[] input, long inputLength, byte[] nonce, byte[] secret);

        static unsafe int Encrypt(byte[] input, long inputLength, byte[] output, int outputOffset, byte[] nonce, byte[] secret)
        {
            fixed (byte* outPtr = output)
                return SecretBoxEasy(outPtr + outputOffset, input, inputLength, nonce, secret);
        }

        public VoiceSocket(DiscoreGuildCache guildCache, DiscoreMemberCache memberCache)
        {
            this.guildCache = guildCache;
            this.memberCache = memberCache;

            log = new DiscoreLogger($"VoiceSocket:{guildCache.Value.Name}");

            socket = new DiscoreWebSocket(WebSocketDataType.Json, log.Prefix);

            encoder = new OpusEncoder(48000, 2, 20, null, OpusApplication.MusicOrMixed);

            samplesPerFrame = encoder.InputSamplingRate / 1000 * FRAME_LENGTH;
            sampleSize = (BITS_PER_SAMPLE / 8) * encoder.InputChannels;
            frameSize = samplesPerFrame * sampleSize;

            sendBuffer = new CircularBuffer((int)Math.Ceiling(BUFFER_LENGTH / (double)FRAME_LENGTH) * frameSize);

            socket.OnMessageReceived += VoiceSocket_OnMessageReceived;
            socket.OnError += Socket_OnError;
        }

        void Reset()
        {
            cancelTokenSource = new CancellationTokenSource();

            sequence = 0;

            sendBuffer.Reset();
        }

        void CreateThreads()
        {
            sendThread = new Thread(SendLoop);
            sendThread.Name = $"VoiceSocket:{guildCache.Value.Name} Send Thread";
            sendThread.IsBackground = true;

            heartbeatThread = new Thread(HeartbeatLoop);
            heartbeatThread.Name = $"VoiceSocket:{guildCache.Value.Name} Heartbeat Thread";
            heartbeatThread.IsBackground = true;
        }

        private void Socket_OnError(object sender, Exception ex)
        {
            HandleFatalError(ex);
        }

        private void UdpSocket_OnError(object sender, Exception ex)
        {
            udpSocket.OnError -= UdpSocket_OnError;
            udpSocket.OnIPDiscovered -= UdpSocket_OnIPDiscovered;

            HandleFatalError(ex);
        }

        private void VoiceSocket_OnMessageReceived(object sender, DiscordApiData e)
        {
            VoiceSocketOPCode op = (VoiceSocketOPCode)e.GetInteger("op");
            DiscordApiData d = e.Get("d");

            switch (op)
            {
                case VoiceSocketOPCode.Ready:
                    HandleReadyPayload(d);
                    break;
                case VoiceSocketOPCode.SessionDescription:
                    HandleSessionDescriptionPayload(d);
                    break;
                case VoiceSocketOPCode.Speaking:
                    break;
                case VoiceSocketOPCode.Heartbeat:
                    break;
                default:
                    log.LogWarning($"Unhandled op code '{op}'");
                    break;
            }
        }

        public bool Connect(string endpoint, string token)
        {
            if (socket.State != WebSocketState.Open && socket.State != WebSocketState.Connecting)
            {
                endpoint = endpoint.Split(':')[0];
                this.endpoint = endpoint;
                this.token = token;

                Reset();

                string uri = $"wss://{endpoint}";
                log.LogVerbose($"Connecting to voice websocket {uri}...");
                if (socket.Connect(uri))
                {
                    log.LogVerbose($"Connected to voice websocket {uri}.");

                    // Create new threads
                    CreateThreads();

                    // Start the threads
                    heartbeatThread.Start();
                    sendThread.Start();

                    // Send the identify payload
                    SendIdentifyPayload();
                    return true;
                }
            }

            return false;
        }

        public bool Disconnect()
        {
            if (socket.State == WebSocketState.Open)
            {
                // Cancel any async operations
                cancelTokenSource.Cancel();

                // Close the socket if the error wasn't directly from the socket.
                socket.Disconnect();

                // Close the UDP socket if still open
                if (udpSocket != null && udpSocket.IsConnected)
                    udpSocket.Disconnect();

                return true;
            }

            return false;
        }

        public void JoinThreads()
        {
            heartbeatThread?.Join();
            sendThread?.Join();

            socket.JoinThreads();
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

        void SendPayload(VoiceSocketOPCode op, DiscordApiData data)
        {
            DiscordApiData payload = new DiscordApiData();
            payload.Set("op", (int)op);
            payload.Set("d", data);

            socket.Send(payload);
        }

        public void SetSpeaking(bool speaking)
        {
            DiscordApiData data = new DiscordApiData();
            data.Set("speaking", speaking);
            data.Set("delay", 0);

            SendPayload(VoiceSocketOPCode.Speaking, data);
        }

        void SendIdentifyPayload()
        {
            DiscordApiData data = new DiscordApiData();
            data.Set("server_id", guildCache.Value.Id);
            data.Set("user_id", memberCache.Value.User.Id);
            data.Set("session_id", memberCache.VoiceState.SessionId);
            data.Set("token", token);

            log.LogVerbose($"[Identify] Sending identify with server_id: {guildCache.Value.Id}");
            SendPayload(VoiceSocketOPCode.Identify, data);
        }

        void SendHeartbeat()
        {
            SendPayload(VoiceSocketOPCode.Heartbeat, null);
        }

        void SendSelectProtocol(string ip, int port)
        {
            DiscordApiData selectProtocol = new DiscordApiData();
            selectProtocol.Set("protocol", "udp");
            DiscordApiData data = selectProtocol.Set("data", DiscordApiData.ContainerType);
            data.Set("address", ip);
            data.Set("port", port);
            data.Set("mode", "xsalsa20_poly1305");

            log.LogVerbose($"[SelectProtocol] Sending select protocol to {ip}:{port}.");
            SendPayload(VoiceSocketOPCode.SelectProtocol, selectProtocol);
        }

        void HandleReadyPayload(DiscordApiData data)
        {
            ssrc = data.GetInteger("ssrc") ?? 0;
            int port = data.GetInteger("port") ?? 0;
            heartbeatInterval = data.GetInteger("heartbeat_interval") ?? 0;

            log.LogVerbose($"[Ready] ssrc: {ssrc}, port: {port}");

            udpSocket = new VoiceUDPSocket(guildCache, endpoint, port);
            udpSocket.OnIPDiscovered += UdpSocket_OnIPDiscovered;
            udpSocket.OnError += UdpSocket_OnError;

            StartIPDiscovery();
        }

        void StartIPDiscovery()
        {
            log.LogVerbose("[IPDiscovery] Starting ip discovery...");

            udpSocket.StartIPDiscovery(ssrc);
        }

        private void UdpSocket_OnIPDiscovered(object sender, IPDiscoveryEventArgs e)
        {
            log.LogVerbose($"[IPDiscovery] IP has been discovered. Endpoint: {e.Ip}:{e.Port}");
            SendSelectProtocol(e.Ip, e.Port);
        }

        void HandleSessionDescriptionPayload(DiscordApiData data)
        {
            IList<DiscordApiData> secretKey = data.GetArray("secret_key");
            key = new byte[secretKey.Count];
            for (int i = 0; i < secretKey.Count; i++)
                key[i] = (byte)secretKey[i].ToInteger();

            log.LogVerbose($"[SessionDescription] Protocol succesfully selected, using mode '{data.GetString("mode")}'");

            readyToSendVoice = true;
        }

        void SendLoop()
        {
            //const int TICKS_PER_S = 1000;
            const int TICKS_PER_MS = 1;

            try
            {
                // Wait for full connection
                while ((!readyToSendVoice || socket.State != WebSocketState.Open) && !cancelTokenSource.IsCancellationRequested)
                    Thread.Sleep(1000);

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
                while (socket.State == WebSocketState.Open && !cancelTokenSource.IsCancellationRequested)
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
                        // If we have a frame to send
                        if (hasFrame)
                        {
                            hasFrame = false;
                            // Send the frame across UDP
                            //udpSocket.Send(voicePacket, rtpPacketLength).Wait();
                            udpSocket.Send(voicePacket, rtpPacketLength);
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
                        Thread.Sleep(1);
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex);
                HandleFatalError(ex);
            }
        }

        void HeartbeatLoop()
        {
            try
            {
                // Wait for heartbeat interval
                while (heartbeatInterval == 0 && (!readyToSendVoice || socket.State != WebSocketState.Open)
                     && !cancelTokenSource.IsCancellationRequested)
                    Thread.Sleep(1000);

                // Heartbeat
                while (socket.State == WebSocketState.Open && !cancelTokenSource.IsCancellationRequested)
                {
                    SendHeartbeat();
                    Thread.Sleep(heartbeatInterval);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex);
                HandleFatalError(ex);
            }
        }

        void HandleFatalError(Exception ex)
        {
            Disconnect();

            OnError?.Invoke(this, ex);
        }

        public void Dispose()
        {
            cancelTokenSource?.Cancel();

            if (udpSocket != null)
            {
                udpSocket.OnIPDiscovered -= UdpSocket_OnIPDiscovered;
                udpSocket.Dispose();
            }

            socket.Dispose();
        }
    }
}
