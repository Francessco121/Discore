using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Audio
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

        DiscordClient client;
        //UdpClient socket;
        Socket socket;
        bool isAwaitingIPDiscovery;
        IPEndPoint endpoint;

        Thread receiveThread;
        CancellationTokenSource cancelTokenSource;

        public VoiceUDPSocket(DiscordClient client, string hostname, int port)
        {
            this.client = client;

            cancelTokenSource = new CancellationTokenSource();

            receiveThread = new Thread(ReceiveLoop);
            receiveThread.Name = "VoiceUDPSocket Receive Thread";
            receiveThread.IsBackground = true;

            IPAddress ip = Dns.GetHostAddressesAsync(hostname).Result.FirstOrDefault();
            endpoint = new IPEndPoint(ip, port);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(endpoint);
            
            try
            {
                socket.DontFragment = true;
            }
            catch (Exception ex)
            {
                DiscordLogger.Default.LogError($"[VoiceUDPSocket] Failed to set DontFragment: {ex}");
            }

            //socket = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            //socket.Ttl = 128;

            receiveThread.Start();
        }

        public void /*async Task*/ StartIPDiscovery(int ssrc)
        {
            byte[] packet = new byte[70];
            packet[0] = (byte)(ssrc >> 24);
            packet[1] = (byte)(ssrc >> 16);
            packet[2] = (byte)(ssrc >> 8);
            packet[3] = (byte)(ssrc >> 0);

            isAwaitingIPDiscovery = true;
            //await Send(packet);
            Send(packet);
        }

        //public async Task Send(byte[] data)
        //{
        //    await socket.SendAsync(data, data.Length, endpoint).ConfigureAwait(false);
        //}

        //public async Task Send(byte[] data, int bytes)
        //{
        //    await socket.SendAsync(data, bytes, endpoint).ConfigureAwait(false);
        //}

        public void Send(byte[] data)
        {
            socket.SendTo(data, endpoint);
        }

        public void Send(byte[] data, int bytes)
        {
            socket.SendTo(data, bytes, SocketFlags.None, endpoint);
        }

        /*async*/ void ReceiveLoop()
        {
            try
            {
                byte[] buffer = new byte[socket.ReceiveBufferSize];

                //while (!cancelTokenSource.IsCancellationRequested && socket.Client != null)
                while (!cancelTokenSource.IsCancellationRequested && socket != null)
                {
                    EndPoint endpoint = this.endpoint;
                    //socket.Bind(endpoint);
                    int read = socket.Receive(buffer);
                    if (read == 70 && isAwaitingIPDiscovery)
                        HandleIPDiscoveryPacket(buffer);

                    //UdpReceiveResult result = await socket.ReceiveAsync().ConfigureAwait(true);
                    //if (result.Buffer.Length == 70 && isAwaitingIPDiscovery)
                    //    HandleIPDiscoveryPacket(result.Buffer);
                }
            }
            catch (ObjectDisposedException) { }
            catch (SocketException ex)
            {
                // Ignore interrupted errors, it just means the socket was disconnected
                // while the socket was waiting for data on Socket.Receive().
                if (ex.SocketErrorCode != SocketError.Interrupted)
                    DiscordLogger.Default.LogError($"{ex.SocketErrorCode}: {ex}");
            }
            catch (Exception e)
            {
                client.EnqueueError(e);
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

        public void Dispose()
        {
            cancelTokenSource.Cancel();
            socket.Dispose();
        }
    }
}
