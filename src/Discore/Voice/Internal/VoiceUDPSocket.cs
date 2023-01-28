using Nito.AsyncEx;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Voice.Internal
{
    /**
     * REUSABILITY:
     * This class is mostly reusable already, the only part missing is guaranteeing that
     * the send/receive tasks have completed before initiating a new connection. Currently,
     * neither is waited for when disconnecting as there is no way to cancel a send/receive
     * operation on a System.Net.Sockets.Socket yet.
    **/

    class VoiceUdpSocket : IDisposable
    {
        public event EventHandler? OnClosedPrematurely;

        public bool IsConnected => socket != null && socket.Connected;

        public int BytesToSend => sendBuffer.Count;

        public AsyncCollection<IPDiscoveryEventArgs> IPDiscoveryQueue { get; } =
            new AsyncCollection<IPDiscoveryEventArgs>();

        public int Ssrc { get; }

        public bool IsPaused { get; set; }

        bool isDisposed;

        Socket? socket;
        IPEndPoint? endPoint;

        Thread? sendThread;
        Task? receiveTask;

        bool discoveringIP;
        byte[]? secretKey;

        readonly OpusEncoder encoder;
        readonly CircularBuffer sendBuffer;
        readonly DiscoreLogger log;

        public VoiceUdpSocket(string loggingName, int ssrc)
        {
            log = new DiscoreLogger(loggingName);

            Ssrc = ssrc;

            encoder = new OpusEncoder(
                samplingRate: 48000, 
                channels: 2, 
                frameLength: 20, 
                bitrate: null, 
                application: OpusApplication.MusicOrMixed);

            // Create send buffer
            const int FRAME_LENGTH = 20;
            const int BITS_PER_SAMPLE = 16;
            const int BUFFER_LENGTH = 1000;

            int samplesPerFrame = encoder.InputSamplingRate / 1000 * FRAME_LENGTH;
            int sampleSize = (BITS_PER_SAMPLE / 8) * encoder.InputChannels;
            int frameSize = samplesPerFrame * sampleSize;

            sendBuffer = new CircularBuffer((int)Math.Ceiling(BUFFER_LENGTH / (double)FRAME_LENGTH) * frameSize);

            // Init libsodium
            if (!LibSodium.Init())
                throw new InvalidOperationException("Failed to initialize libsodium.");
        }

        /// <exception cref="InvalidOperationException">Thrown if the socket is already connected.</exception>
        /// <exception cref="SocketException">Thrown if the socket fails to connect.</exception>
        public async Task ConnectAsync(IPAddress ip, int port)
        {
            if (socket != null && socket.Connected)
                throw new InvalidOperationException("The UDP socket is already connected!");

            endPoint = new IPEndPoint(ip, port);

            // Connect the socket
            socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
            socket.SendTimeout = 1000 * 10;
            socket.ReceiveTimeout = 1000 * 10;
            
            await socket.ConnectAsync(endPoint).ConfigureAwait(false);

            // At this point, the socket has successfully connected
            receiveTask = ReceiveLoop();
        }

        public void Shutdown()
        {
            try
            {
                socket?.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException ex)
            {
                // If this occurs then the socket is already borked,
                // technically shouldn't happen though.
                log.LogError($"[Shutdown] Unexpected error: code = {ex.SocketErrorCode}, error = {ex}");
            }
        }

        /// <summary>
        /// Initializes the UDP send loop.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Start(byte[] secretKey)
        {
            if (sendThread != null && sendThread.ThreadState.HasFlag(ThreadState.Running))
                throw new InvalidOperationException("The UDP socket send loop is already running!");

            this.secretKey = secretKey;

            sendThread = new Thread(SendLoop);
            sendThread.Name = $"[{log.Prefix}] Send Thread";
            sendThread.IsBackground = true;
            sendThread.Start();
        }

        /// <summary>
        /// Writes the specified data to a buffer to be sent over the UDP socket.
        /// </summary>
        public void SendData(byte[] data, int offset, int count)
        {
            sendBuffer.Write(data, offset, count);
        }

        /// <summary>
        /// Gets whether the specified number of bytes is free in the buffer.
        /// </summary>
        public bool CanSendData(int size)
        {
            return sendBuffer.MaxLength - sendBuffer.Count >= size;
        }

        /// <summary>
        /// Clears the buffer.
        /// </summary>
        public void ClearVoiceBuffer()
        {
            sendBuffer.Reset();
        }

        #region Receiving
        async Task HandleIPDiscoveryPacket(byte[] data)
        {
            discoveringIP = false;

            // Read IP as null-terminated string
            string ip = Encoding.UTF8.GetString(data, 4, 70 - 6).TrimEnd('\0');

            // Read port
            int port = (ushort)(data[68] | data[69] << 8);

            await IPDiscoveryQueue.AddAsync(new IPDiscoveryEventArgs(ip, port)).ConfigureAwait(false);
        }

        async Task ReceiveLoop()
        {
            var buffer = new ArraySegment<byte>(new byte[socket!.ReceiveBufferSize]);
            
            while (socket.Connected)
            {
                try
                {
                    int read = await socket.ReceiveAsync(buffer, SocketFlags.None)
                        .ConfigureAwait(false);

                    if (read == 70 && discoveringIP)
                    {
                        await HandleIPDiscoveryPacket(buffer.Array!);

                        // For now, the receive loop is only needed for discovering the IP.
                        // To save from some unneeded calculations, we can end the loop here.
                        // TODO: remove when voice data receiving support is added.
                        break;
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Socket was disposed while receiving
                    break;
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                        // Since we have the receive timeout set to a finite value,
                        // if the bot is deafened, or has no one in the voice channel with it,
                        // it will time out on receiving, but this is okay.
                        continue;
                    else if (ex.SocketErrorCode == SocketError.Interrupted || ex.SocketErrorCode == SocketError.Shutdown)
                    {
                        // Socket was shutdown, just end loop.
                        break;
                    }
                    else
                    {
                        // Unexpected error
                        log.LogError($"[ReceiveLoop] Unexpected socket error: code = {ex.SocketErrorCode}, error = {ex}");
                        OnClosedPrematurely?.Invoke(this, EventArgs.Empty);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    // Unexpected error
                    log.LogError($"[ReceiveLoop] Unexpected error: {ex}");
                    OnClosedPrematurely?.Invoke(this, EventArgs.Empty);
                    break;
                }
            }
        }
        #endregion

        #region Sending
        Task SendAsync(ArraySegment<byte> data)
        {
            return socket!.SendAsync(data, SocketFlags.None);
        }

        void Send(byte[] buffer, int offset, int count)
        {
            socket!.Send(buffer, offset, count, SocketFlags.None);
        }

        /// <exception cref="SocketException">Thrown if the socket encounters an error while sending data.</exception>
        public Task StartIPDiscoveryAsync()
        {
            byte[] packet = new byte[70];
            packet[0] = (byte)(Ssrc >> 24);
            packet[1] = (byte)(Ssrc >> 16);
            packet[2] = (byte)(Ssrc >> 8);
            packet[3] = (byte)(Ssrc >> 0);

            discoveringIP = true;
            return SendAsync(new ArraySegment<byte>(packet));
        }

        void SendLoop()
        {
            const int TICKS_PER_MS = 1;
            const int MAX_OPUS_SIZE = 4000;

            byte[] frame = new byte[encoder.FrameSize];
            byte[] encodedFrame = new byte[MAX_OPUS_SIZE];

            ushort sequence = 0;

            uint timestamp = 0;

            int ticksPerFrame = TICKS_PER_MS * encoder.FrameLength;
            uint samplesPerFrame = (uint)encoder.SamplesPerFrame;

            byte[] nonce = new byte[24];
            byte[] voicePacket = new byte[MAX_OPUS_SIZE + 12 + 16];

            int rtpPacketLength = 0;

            // Setup RTP packet header
            voicePacket[0] = 0x80; // Packet Type
            voicePacket[1] = 0x78; // Packet Version
            voicePacket[8] = (byte)(Ssrc >> 24); // ssrc
            voicePacket[9] = (byte)(Ssrc >> 16);
            voicePacket[10] = (byte)(Ssrc >> 8);
            voicePacket[11] = (byte)(Ssrc >> 0);

            // Copy RTP packet header into nonce
            Buffer.BlockCopy(voicePacket, 0, nonce, 0, 12);

            int nextTicks = Environment.TickCount;

            // Begin send loop
            bool hasFrame = false;
            while (socket!.Connected)
            {
                try
                {
                    // If we don't have a frame to send and we have a frame buffered
                    if (!hasFrame && sendBuffer.Count > 0)
                    {
                        // Read frame from buffer
                        sendBuffer.Read(frame, 0, frame.Length);

                        // Set sequence number in RTP packet
                        voicePacket[2] = (byte)(sequence >> 8);
                        voicePacket[3] = (byte)(sequence >> 0);
                        // Set timestamp in RTP packet
                        voicePacket[4] = (byte)(timestamp >> 24);
                        voicePacket[5] = (byte)(timestamp >> 16);
                        voicePacket[6] = (byte)(timestamp >> 8);
                        voicePacket[7] = (byte)(timestamp >> 0);

                        // Increase the sequence number, use unchecked because wrapping is valid
                        unchecked { sequence++; };

                        // Encode the frame
                        int encodedLength = encoder.EncodeFrame(frame, 0, encodedFrame);

                        // Update the separately stored nonce from RTP packet
                        Buffer.BlockCopy(voicePacket, 2, nonce, 2, 6);

                        // Encrypt the frame
                        int encryptStatus = LibSodium.Encrypt(encodedFrame, encodedLength, voicePacket, 12, nonce, secretKey!);
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
                            log.LogError($"[SendLoop] Failed to encrypt RTP packet. encryptStatus = {encryptStatus}");
                        }
                    }

                    int currentTicks = Environment.TickCount;
                    int ticksToNextFrame = nextTicks - currentTicks;

                    // Is it time to send the next frame?
                    if (ticksToNextFrame <= 0)
                    {
                        if (IsPaused)
                        {
                            // If we are paused, do nothing
                            Thread.Sleep(100);
                        }
                        // If we have a frame to send
                        else if (hasFrame)
                        {
                            hasFrame = false;

                            try
                            {
                                // Send the frame across UDP
                                Send(voicePacket, 0, rtpPacketLength);
                            }
                            catch (ObjectDisposedException)
                            {
                                // Socket was disposed while sending
                                break;
                            }
                            catch (SocketException ex)
                            {
                                if (ex.SocketErrorCode == SocketError.Interrupted || ex.SocketErrorCode == SocketError.Shutdown)
                                {
                                    // Socket was shutdown while sending
                                    break;
                                }
                                else
                                {
                                    // Unexpected error
                                    log.LogError($"[SendLoop] Unexpected socket error: code = {ex.SocketErrorCode}, error = {ex}");
                                    OnClosedPrematurely?.Invoke(this, EventArgs.Empty);
                                    break;
                                }
                            }
                        }

                        // Calculate the time for next frame
                        nextTicks += ticksPerFrame;
                    }
                    else
                    {
                        // Nothing to do, so sleep
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    // Unexpected error
                    log.LogError($"[SendLoop] Unexpected error: {ex}");
                    OnClosedPrematurely?.Invoke(this, EventArgs.Empty);
                    break;
                }
            }
        }
        #endregion

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                // This will also shutdown the socket
                socket?.Dispose();
            }
        }
    }
}
