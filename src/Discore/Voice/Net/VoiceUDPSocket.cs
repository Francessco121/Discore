using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Voice.Net
{
    class IPDiscoveryEventArgs : EventArgs
    {
        public string Ip;
        public int Port;

        public IPDiscoveryEventArgs(string ip, int port)
        {
            Ip = ip;
            Port = port;
        }
    }

    class VoiceUDPSocket : IDisposable
    {
        public event EventHandler<IPDiscoveryEventArgs> OnIPDiscovered;
        public event EventHandler<Exception> OnError;

        public bool IsConnected { get { return socket.Connected; } }

        DiscoreLogger log;

        Socket socket;
        string hostname;
        int port;
        IPEndPoint endpoint;
        bool isAwaitingIPDiscovery;

        Task receiveTask;

        public VoiceUDPSocket(DiscoreGuildCache guildCache, string hostname, int port)
        {
            this.hostname = hostname;
            this.port = port;

            log = new DiscoreLogger($"VoiceUDPSocket:{guildCache.Value.Name}");
        }

        public async Task ConnectAsync()
        {
            IPAddress ip = (await Dns.GetHostAddressesAsync(hostname)).FirstOrDefault();
            endpoint = new IPEndPoint(ip, port);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SendTimeout = 1000 * 10;
            socket.ReceiveTimeout = 1000 * 10;
            socket.Connect(endpoint);

            receiveTask = new Task(ReceiveLoop);
            receiveTask.Start();
        }

        public void Shutdown()
        {
            try { socket.Shutdown(SocketShutdown.Both); }
            catch { }
        }

        public async Task StartIPDiscoveryAsync(int ssrc)
        {
            byte[] packet = new byte[70];
            packet[0] = (byte)(ssrc >> 24);
            packet[1] = (byte)(ssrc >> 16);
            packet[2] = (byte)(ssrc >> 8);
            packet[3] = (byte)(ssrc >> 0);

            isAwaitingIPDiscovery = true;
            await SendAsync(packet);
        }

        public async Task SendAsync(byte[] data)
        {
            await SendAsync(data, data.Length);
        }

        public async Task SendAsync(byte[] data, int bytes)
        {
            try
            {
                ArraySegment<byte> segment = new ArraySegment<byte>(data, 0, bytes);
                await socket.SendToAsync(segment, SocketFlags.None, endpoint);
            }
            catch (SocketException ex)
            {
                // Ignore interrupted/shutdown errors, it just means the socket was disconnected
                // while the socket was waiting for data on Socket.Receive().
                if (ex.SocketErrorCode != SocketError.Interrupted && ex.SocketErrorCode != SocketError.Success
                    && ex.SocketErrorCode != SocketError.Shutdown)
                {
                    throw;
                }
            }
        }

        async void ReceiveLoop()
        {
            try
            {
                byte[] buffer = new byte[socket.ReceiveBufferSize];
                ArraySegment<byte> bufferTarget = new ArraySegment<byte>(buffer);

                while (socket != null && socket.Connected)
                {

                    try
                    {
                        int read = await socket.ReceiveAsync(bufferTarget, SocketFlags.None);
                        if (read == 70 && isAwaitingIPDiscovery)
                            HandleIPDiscoveryPacket(buffer);
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode != SocketError.TimedOut
                            && ex.SocketErrorCode != SocketError.IOPending)
                            // Since we have the receive timeout set to a finite value,
                            // if the bot is deafened, or has no one in the voice channel with it,
                            // it will time out on receiving, but this is okay.

                            // We also don't want to throw for an IO pending error,
                            // this happens everytime the socket is shutdown while
                            // still waiting to receive.
                            throw;
                    }
                }
            }
            catch (ObjectDisposedException) { }
            catch (SocketException ex)
            {
                // Ignore interrupted/shutdown errors, it just means the socket was disconnected
                // while the socket was waiting for data on Socket.Receive().
                if (ex.SocketErrorCode != SocketError.Interrupted && ex.SocketErrorCode != SocketError.Success
                    && ex.SocketErrorCode != SocketError.Shutdown)
                {
                    log.LogError($"{ex.SocketErrorCode}: {ex}");
                    HandleFatalError(ex);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex);
                HandleFatalError(ex);
            }
        }

        void HandleIPDiscoveryPacket(byte[] data)
        {
            isAwaitingIPDiscovery = false;

            // Read null-terminated string
            string ip = Encoding.UTF8.GetString(data, 4, 70 - 6).TrimEnd('\0');

            // Read port
            int port = (ushort)(data[68] | data[69] << 8);

            OnIPDiscovered?.Invoke(this, new IPDiscoveryEventArgs(ip, port));
        }

        void HandleFatalError(Exception ex)
        {
            Shutdown();
            OnError?.Invoke(this, ex);
        }

        public void Dispose()
        {
            socket.Dispose();
        }
    }
}
