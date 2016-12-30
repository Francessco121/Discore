using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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
        IPEndPoint endpoint;
        bool isAwaitingIPDiscovery;

        Thread receiveThread;

        public VoiceUDPSocket(DiscoreGuildCache guildCache, string hostname, int port)
        {
            log = new DiscoreLogger($"VoiceUDPSocket:{guildCache.Value.Name}");

            IPAddress ip = Dns.GetHostAddressesAsync(hostname).Result.FirstOrDefault();
            endpoint = new IPEndPoint(ip, port);

            receiveThread = new Thread(ReceiveLoop);
            receiveThread.Name = "VoiceUDPSocket Receive Thread";
            receiveThread.IsBackground = true;

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SendTimeout = 1000 * 10;
            socket.ReceiveTimeout = 1000 * 10;
            socket.Connect(endpoint);

            receiveThread.Start();
        }

        public void Disconnect()
        {
            try { socket.Shutdown(SocketShutdown.Both); }
            catch { }
        }

        public void StartIPDiscovery(int ssrc)
        {
            byte[] packet = new byte[70];
            packet[0] = (byte)(ssrc >> 24);
            packet[1] = (byte)(ssrc >> 16);
            packet[2] = (byte)(ssrc >> 8);
            packet[3] = (byte)(ssrc >> 0);

            isAwaitingIPDiscovery = true;
            Send(packet);
        }

        public void Send(byte[] data)
        {
            Send(data, data.Length);
        }

        public void Send(byte[] data, int bytes)
        {
            try
            {
                socket.SendTo(data, bytes, SocketFlags.None, endpoint);
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

        void ReceiveLoop()
        {
            try
            {
                byte[] buffer = new byte[socket.ReceiveBufferSize];

                while (socket != null && socket.Connected)
                {

                    try
                    {
                        int read = socket.Receive(buffer);
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
            // Shutdown the socket
            try { socket.Shutdown(SocketShutdown.Both); }
            catch { }

            OnError?.Invoke(this, ex);
        }

        public void Dispose()
        {
            socket.Dispose();
        }
    }
}
