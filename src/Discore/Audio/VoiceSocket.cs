using Discore.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Audio
{
    class VoiceSocket : IDisposable
    {
        public DiscordGuildMember Member { get { return member; } }

        DiscordClientWebSocket socket;
        DiscordClient client;
        DiscordGuildMember member;
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
        DiscordLogger log;

        public VoiceSocket(DiscordClient client, DiscordGuildMember member)
        {
            this.client = client;
            this.member = member;

            log = new DiscordLogger($"VoiceSocket:{member.Guild.Name}");

            socket = new DiscordClientWebSocket(client);

            sendThread = new Thread(SendLoop);
            sendThread.Name = "VoiceSocket Send Thread";
            sendThread.IsBackground = true;

            heartbeatThread = new Thread(HeartbeatLoop);
            heartbeatThread.Name = "VoiceSocket Heartbeat Thread";
            heartbeatThread.IsBackground = true;

            cancelTokenSource = new CancellationTokenSource();

            encoder = new OpusEncoder(48000, 2, 20, null, OpusApplication.MusicOrMixed);

            samplesPerFrame = encoder.InputSamplingRate / 1000 * FRAME_LENGTH;
            sampleSize = (BITS_PER_SAMPLE / 8) * encoder.InputChannels;
            frameSize = samplesPerFrame * sampleSize;

            sendBuffer = new CircularBuffer((int)Math.Ceiling(BUFFER_LENGTH / (double)FRAME_LENGTH) * frameSize);

            socket.OnMessageReceived += VoiceSocket_OnMessageReceived;

            heartbeatThread.Start();
            sendThread.Start();
        }

        private async void VoiceSocket_OnMessageReceived(object sender, DiscordApiData e)
        {
            VoiceSocketOPCode op = (VoiceSocketOPCode)e.GetInteger("op");
            DiscordApiData d = e.Get("d");

            switch (op)
            {
                case VoiceSocketOPCode.Ready:
                    await HandleReadyPayload(d);
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

        public async Task<bool> Connect(string endpoint, string token)
        {
            if (socket.State != WebSocketState.Open)
            {
                endpoint = endpoint.Split(':')[0];
                this.endpoint = endpoint;
                this.token = token;

                string uri = $"wss://{endpoint}";
                log.LogVerbose($"Connecting to voice websocket {uri}...");
                if (await socket.Connect(uri))
                {
                    log.LogInfo($"Connected to voice websocket {uri}.");
                    SendIdentifyPayload();
                    return true;
                }
                else
                    return false;
            }
            return false;
        }

        /// <exception cref="OperationCanceledException"></exception>
        public void SendPCMData(byte[] data, int offset, int count)
        {
            //sendBuffer.Push(data, offset, count, cancelTokenSource.Token);
            sendBuffer.Write(data, offset, count);
        }

        public bool CanSendData(int size)
        {
            return sendBuffer.MaxLength - sendBuffer.Count >= size;
        }

        void SendPayload(VoiceSocketOPCode op, DiscordApiData data)
        {
            DiscordApiData payload = new DiscordApiData();
            payload.Set("op", (int)op);
            payload.Set("d", data);

            socket.Send(payload.SerializeToJson());
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
            data.Set("server_id", ulong.Parse(member.Guild.Id));
            data.Set("user_id", ulong.Parse(member.User.Id));
            data.Set("session_id", member.VoiceState.SessionId);
            data.Set("token", token);

            log.LogVerbose($"[IDENTIFY] Sending identify with server_id: {member.Guild.Id}");
            SendPayload(VoiceSocketOPCode.Identify, data);
        }

        void SendHeartbeat()
        {
            //log.LogHeartbeat("Sending heartbeat");
            SendPayload(VoiceSocketOPCode.Heartbeat, null);
        }

        void SendSelectProtocol(string ip, int port)
        {
            DiscordApiData selectProtocol = new DiscordApiData();
            selectProtocol.Set("protocol", "udp");
            DiscordApiData data = selectProtocol.Set("data", DiscordApiData.CreateContainer());
            data.Set("address", ip);
            data.Set("port", port);
            data.Set("mode", "xsalsa20_poly1305");

            log.LogVerbose($"[SELECT_PROTOCOL] Sending select protocol to {ip}:{port}.");
            SendPayload(VoiceSocketOPCode.SelectProtocol, selectProtocol);
        }

        async Task HandleReadyPayload(DiscordApiData data)
        {
            ssrc = data.GetInteger("ssrc") ?? 0;
            int port = data.GetInteger("port") ?? 0;
            heartbeatInterval = data.GetInteger("heartbeat_interval") ?? 0;

            log.LogVerbose($"[READY] ssrc: {ssrc}, port: {port}");

            udpSocket = new VoiceUDPSocket(client, endpoint, port);
            udpSocket.OnIPDiscovered += UdpSocket_OnIPDiscovered;
            await StartIPDiscovery();
        }

        async Task StartIPDiscovery()
        {
            log.LogVerbose("[IP_DISCOVERY] Starting ip discovery...");
            await udpSocket.StartIPDiscovery(ssrc);
        }

        private void UdpSocket_OnIPDiscovered(object sender, IPDiscoveryEventArgs e)
        {
            log.LogVerbose($"[IP_DISCOVERY] IP has been discovered. Endpoint: {e.Ip}:{e.Port}");
            SendSelectProtocol(e.Ip, e.Port);
        }

        void HandleSessionDescriptionPayload(DiscordApiData data)
        {
            IList<DiscordApiData> secretKey = data.GetArray("secret_key");
            key = new byte[secretKey.Count];
            for (int i = 0; i < secretKey.Count; i++)
                key[i] = (byte)secretKey[i].ToInteger();

            log.LogVerbose($"[SESSION_DESCRIPTION] Protocol succesfully selected, using mode '{data.GetString("mode")}'");

            readyToSendVoice = true;
        }

        [DllImport("libsodium", EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int SecretBoxEasy(byte* output, byte[] input, long inputLength, byte[] nonce, byte[] secret);

        static unsafe int Encrypt(byte[] input, long inputLength, byte[] output, int outputOffset, byte[] nonce, byte[] secret)
        {
            fixed (byte* outPtr = output)
                return SecretBoxEasy(outPtr + outputOffset, input, inputLength, nonce, secret);
        }

        /// <summary>
        /// Modified directly from:
        /// https://github.com/RogueException/Discord.Net/blob/master/src/Discord.Net.Audio/Net/VoiceSocket.cs#L263
        /// TODO: Custom original implementation.
        /// </summary>
        async void SendLoop()
        {
            try
            {
                while ((!readyToSendVoice || socket.State != WebSocketState.Open) && !cancelTokenSource.IsCancellationRequested)
                    Thread.Sleep(1000);

                byte[] frame = new byte[encoder.FrameSize];
                byte[] encodedFrame = new byte[MAX_OPUS_SIZE];
                byte[] voicePacket, pingPacket, nonce = null;
                uint timestamp = 0;
                double nextTicks = 0.0, nextPingTicks = 0.0;
                long ticksPerSeconds = Stopwatch.Frequency;
                double ticksPerMillisecond = Stopwatch.Frequency / 1000.0;
                double ticksPerFrame = ticksPerMillisecond * encoder.FrameLength;
                double spinLockThreshold = 3 * ticksPerMillisecond;
                uint samplesPerFrame = (uint)encoder.SamplesPerFrame;
                Stopwatch sw = Stopwatch.StartNew();

                nonce = new byte[24];
                voicePacket = new byte[MAX_OPUS_SIZE + 12 + 16];

                pingPacket = new byte[8];

                int rtpPacketLength = 0;
                voicePacket[0] = 0x80; //Flags;
                voicePacket[1] = 0x78; //Payload Type
                voicePacket[8] = (byte)(ssrc >> 24);
                voicePacket[9] = (byte)(ssrc >> 16);
                voicePacket[10] = (byte)(ssrc >> 8);
                voicePacket[11] = (byte)(ssrc >> 0);

                Buffer.BlockCopy(voicePacket, 0, nonce, 0, 12);

                bool hasFrame = false;
                while (socket.State == WebSocketState.Open && !cancelTokenSource.IsCancellationRequested)
                {
                    if (!hasFrame && sendBuffer.Count >= frame.Length)
                    {
                        int read = sendBuffer.Read(frame, 0, frame.Length);

                        ushort sequence = unchecked(this.sequence++);
                        voicePacket[2] = (byte)(sequence >> 8);
                        voicePacket[3] = (byte)(sequence >> 0);
                        voicePacket[4] = (byte)(timestamp >> 24);
                        voicePacket[5] = (byte)(timestamp >> 16);
                        voicePacket[6] = (byte)(timestamp >> 8);
                        voicePacket[7] = (byte)(timestamp >> 0);


                        //Encode
                        int encodedLength = encoder.EncodeFrame(frame, 0, encodedFrame);

                        // Encrypt
                        Buffer.BlockCopy(voicePacket, 2, nonce, 2, 6); //Update nonce
                        int ret = Encrypt(encodedFrame, encodedLength, voicePacket, 12, nonce, key);
                        if (ret != 0)
                            continue;

                        rtpPacketLength = encodedLength + 12 + 16;

                        timestamp = unchecked(timestamp + samplesPerFrame);
                        hasFrame = true;
                    }

                    long currentTicks = sw.ElapsedTicks;
                    double ticksToNextFrame = nextTicks - currentTicks;
                    if (ticksToNextFrame <= 0.0)
                    {
                        if (hasFrame)
                        {
                            await udpSocket.Send(voicePacket, rtpPacketLength).ConfigureAwait(true);
                            hasFrame = false;
                        }

                        nextTicks += ticksPerFrame;

                        //Is it time to send out another ping?
                        if (currentTicks > nextPingTicks)
                        {
                            //Increment in LE
                            for (int i = 0; i < 8; i++)
                            {
                                var b = pingPacket[i];
                                if (b == byte.MaxValue)
                                    pingPacket[i] = 0;
                                else
                                {
                                    pingPacket[i] = (byte)(b + 1);
                                    break;
                                }
                            }
                            await udpSocket.Send(pingPacket, pingPacket.Length).ConfigureAwait(true);
                            nextPingTicks = currentTicks + 5 * ticksPerSeconds;
                        }
                    }
                    else
                    {
                        if (hasFrame)
                        {
                            int time = (int)Math.Floor(ticksToNextFrame / ticksPerMillisecond);
                            if (time > 0)
                                Thread.Sleep(time);
                        }
                        else
                            Thread.Sleep(1); //Give as much time to the encrypter as possible
                    }
                }
            }
            catch (Exception e)
            {
                client.EnqueueError(e);
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
            catch (Exception e)
            {
                client.EnqueueError(e);
            }
        }

        public void Dispose()
        {
            log.LogInfo("Closing...");

            cancelTokenSource.Cancel();

            if (udpSocket != null)
            {
                udpSocket.OnIPDiscovered -= UdpSocket_OnIPDiscovered;
                udpSocket.Dispose();
            }

            socket.Dispose();
        }
    }
}
