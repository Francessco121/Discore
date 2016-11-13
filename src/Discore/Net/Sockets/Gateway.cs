using System;
using System.Net.WebSockets;
using System.Threading;

namespace Discore.Net.Sockets
{
    partial class Gateway : IDisposable
    {
        DiscordApplication app;
        Shard shard;

        DiscoreWebSocket socket;
        DiscoreLogger log;

        int sequence;
        string sessionId;
        int heartbeatInterval;
        int heartbeatTimeoutAt;

        /// <summary>
        /// Maximum number of missed heartbeats before timing out.
        /// </summary>
        const int HEARTBEAT_TIMEOUT_MISSED_PACKETS = 5;

        Thread heartbeatThread;

        bool isDisposed;
        bool isReconnecting;

        const int GATEWAY_VERSION = 5;

        internal Gateway(DiscordApplication app, Shard shard)
        {
            this.app = app;
            this.shard = shard;
               
            log = new DiscoreLogger("Gateway");

            InitializePayloadHandlers();
            InitializeDispatchHandlers();
            
            socket = new DiscoreWebSocket("Gateway");
            socket.OnError += Socket_OnError;
            socket.OnMessageReceived += Socket_OnMessageReceived;
        }

        /// <param name="gatewayResume">Will send a resume payload instead of an identify upon reconnecting when true.</param>
        public bool Connect(bool gatewayResume = false)
        {
            if (socket.State != WebSocketState.Connecting && socket.State != WebSocketState.Open)
            {
                // Reset gateway state
                if (!gatewayResume)
                    Reset();

                // Attempt to connect socket
                // TODO: gateway endpoint should be retrieved from a cache,
                // and GET /gateway should be called when this connect fails
                // to ensure we have the correct endpoint.
                // OR
                // the endpoint can be passed from higher up,
                // since GET /gateway/bot is now a thing and needs to be managed
                // from a higher point.
                if (socket.Connect($"{"wss://gateway.discord.gg/"}/?encoding=json&v={GATEWAY_VERSION}"))
                {
                    if (gatewayResume)
                        SendResumePayload();
                    else
                        SendIdentifyPayload();

                    int timeoutAt = Environment.TickCount + (10 * 1000); // Give Discord 10s to send Hello payload

                    while (heartbeatInterval <= 0 && Environment.TickCount < timeoutAt)
                        Thread.Sleep(1);

                    if (heartbeatInterval > 0)
                    {
                        // Handshake was successful, begin the heartbeat loop
                        heartbeatThread = new Thread(HeartbeatLoop);
                        heartbeatThread.Name = "Gateway Heartbeat Thread";
                        heartbeatThread.IsBackground = true;

                        heartbeatThread.Start();

                        return true;
                    }
                    else
                        // We timed out, but the socket is still connected.
                        socket.Disconnect();
                }
            }
            
            return false;
        }

        public bool Disconnect()
        {
            return socket.Disconnect();
        }

        void Reset()
        {
            sequence = 0;
            heartbeatInterval = 0;

            shard.User = null;
        }

        void HeartbeatLoop()
        {
            // Set timeout
            heartbeatTimeoutAt = Environment.TickCount + (heartbeatInterval * HEARTBEAT_TIMEOUT_MISSED_PACKETS);

            bool timedOut = false;

            while (socket.State == WebSocketState.Open)
            {
                if (Environment.TickCount > heartbeatTimeoutAt)
                {
                    timedOut = true;
                    break;
                }

                SendHeartbeatPayload();
                Thread.Sleep(heartbeatInterval);
            }

            if (timedOut)
            {
                // Attempt reconnect
                socket.Disconnect();
                Reconnect();

                // Once the socket reconnects, we can let the heartbeat thread
                // gracefully end, as it will be overwritten by the new handshake.
            }
        }

        /// <param name="gatewayResume">Whether to perform a full-reconnect or just a resume.</param>
        void Reconnect(bool gatewayResume = false)
        {
            // Since a reconnect can be started from multiple threads,
            // ensure that we do not enter this loop simultaneously.
            if (!isReconnecting)
            {
                isReconnecting = true;

                while (!isDisposed && !Connect(gatewayResume))
                    // Give 5s in between failed connections
                    Thread.Sleep(5000);

                isReconnecting = false;
            }
        }

        private void Socket_OnError(object sender, Exception e)
        {
            // Socket errors are fatal, so attempt to reconnect.
            Reconnect();
        }

        private void Socket_OnMessageReceived(object sender, DiscordApiData e)
        {
            GatewayOPCode op = (GatewayOPCode)e.GetInteger("op");
            DiscordApiData data = e.Get("d");

            PayloadCallback callback;
            if (payloadHandlers.TryGetValue(op, out callback))
                callback(e, data);
            else
                log.LogWarning($"Missing handler for payload: {op}({(int)op})");
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                socket.Dispose();
            }
        }
    }
}
