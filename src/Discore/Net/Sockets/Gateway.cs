using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Net.Sockets
{
    partial class Gateway
    {
        DiscordApplication app;
        Shard shard;

        DiscoreWebSocket socket;
        DiscoreLogger log;

        int sequence;
        int heartbeatInterval;
        int heartbeatTimeoutAt;

        /// <summary>
        /// Maximum number of missed heartbeats before timing out.
        /// </summary>
        const int HEARTBEAT_TIMEOUT_MISSED_PACKETS = 5;

        Thread heartbeatThread;

        internal Gateway(DiscordApplication app, Shard shard)
        {
            this.app = app;
            this.shard = shard;
               
            log = new DiscoreLogger("Gateway");

            InitializePayloadHandlers();
            InitializeDispatchHandlers();
            
            socket = new DiscoreWebSocket("Gateway");
            socket.OnConnected += Socket_OnConnected;
            socket.OnDisconnected += Socket_OnDisconnected;
            socket.OnError += Socket_OnError;
            socket.OnMessageReceived += Socket_OnMessageReceived;
        }

        public bool Connect()
        {
            if (socket.State != WebSocketState.Connecting && socket.State != WebSocketState.Open)
            {
                // Reset gateway state
                Reset();

                // Attempt to connect socket
                if (socket.Connect($"{"wss://gateway.discord.gg/"}/encoding=json&v=5"))
                {
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

        void Reset()
        {
            sequence = 0;
            heartbeatInterval = 0;
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
            }

            if (timedOut)
            {
                // Attempt reconnect
                socket.Disconnect();

                while (!Connect())
                    // Give 5s in between failed connections
                    Thread.Sleep(5000);

                // Once the socket reconnects, we can let the heartbeat thread
                // gracefully end, as it will be overwritten by the new handshake.
            }
        }

        private void Socket_OnConnected(object sender, Uri e)
        {
            
        }

        private void Socket_OnDisconnected(object sender, WebSocketCloseStatus e)
        {
            
        }

        private void Socket_OnError(object sender, Exception e)
        {
            
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
    }
}
