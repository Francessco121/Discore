using System;
using System.Net.WebSockets;
using System.Threading;

namespace Discore.WebSocket.Net
{
    partial class Gateway : IDiscordGateway, IDisposable
    {
        public Shard Shard { get { return shard; } }

        public event EventHandler<GatewayDisconnectCode> OnFatalDisconnection;

        /// <summary>
        /// Maximum number of missed heartbeats before timing out.
        /// </summary>
        const int HEARTBEAT_TIMEOUT_MISSED_PACKETS = 5;

        const int GATEWAY_VERSION = 5;

        DiscordWebSocketApplication app;
        Shard shard;

        DiscoreWebSocket socket;
        DiscoreLogger log;

        int sequence;
        string sessionId;
        int heartbeatInterval;
        int heartbeatTimeoutAt;

        Thread heartbeatThread;

        bool isDisposed;
        bool isReconnecting;

        DiscoreCache cache;

        GatewayRateLimiter connectionRateLimiter;
        GatewayRateLimiter outboundEventRateLimiter;
        GatewayRateLimiter gameStatusUpdateRateLimiter;

        internal Gateway(DiscordWebSocketApplication app, Shard shard)
        {
            this.app = app;
            this.shard = shard;

            cache = shard.Cache;

            string logName = $"Gateway#{shard.Id}";
               
            log = new DiscoreLogger(logName);

            // Up-to-date rate limit parameters: https://discordapp.com/developers/docs/topics/gateway#rate-limiting
            connectionRateLimiter = new GatewayRateLimiter(5 * 1000, 1); // One connection attempt per 5 seconds
            outboundEventRateLimiter = new GatewayRateLimiter(60 * 1000, 120); // 120 outbound events every 60 seconds
            gameStatusUpdateRateLimiter = new GatewayRateLimiter(60 * 1000, 5); // 5 status updates per minute

            InitializePayloadHandlers();
            InitializeDispatchHandlers();
            
            socket = new DiscoreWebSocket(WebSocketDataType.Json, logName);
            socket.OnError += Socket_OnError;
            socket.OnMessageReceived += Socket_OnMessageReceived;
        }

        /// <param name="forceFindNew">Whether to call the HTTP forcefully, or use the local cached value.</param>
        string GetGatewayUrl(bool forceFindNew = false)
        {
            DiscoreLocalStorage localStorage = DiscoreLocalStorage.Instance;

            string gatewayUrl = localStorage.GatewayUrl;
            if (forceFindNew || string.IsNullOrWhiteSpace(gatewayUrl))
            {
                try
                {
                    DiscordApiData getData = app.HttpApi.InternalApi.Gateway.Get().Result;
                    gatewayUrl = getData.GetString("url");
                }
                catch (AggregateException aex) { throw aex.InnerException; }

                localStorage.GatewayUrl = gatewayUrl;
                localStorage.Save();
            }

            return gatewayUrl;
        }

        /// <param name="gatewayResume">Will send a resume payload instead of an identify upon reconnecting when true.</param>
        public bool Connect(bool gatewayResume = false)
        {
            if (socket.State != WebSocketState.Connecting && socket.State != WebSocketState.Open)
            {
                // Reset gateway state
                if (!gatewayResume)
                    Reset();

                string gatewayUrl = GetGatewayUrl();

                connectionRateLimiter.Invoke(); // Check with the connection rate limiter.
                if (socket.Connect($"{gatewayUrl}/?encoding=json&v={GATEWAY_VERSION}"))
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
                else
                {
                    // Since we failed to connect, try and find the new gateway url.
                    string newGatewayUrl = GetGatewayUrl(true);
                    if (gatewayUrl != newGatewayUrl)
                    {
                        // If the endpoint did change, overwrite it in storage.
                        DiscoreLocalStorage localStorage = DiscoreLocalStorage.Instance;
                        localStorage.GatewayUrl = newGatewayUrl;

                        localStorage.Save();
                    }
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

                while (!isDisposed)
                {
                    try
                    {
                        if (Connect(gatewayResume))
                            break;
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"[Reconnect] {ex}");
                    }
                }

                isReconnecting = false;
            }
        }

        private void Socket_OnError(object sender, Exception e)
        {
            DiscoreWebSocketException dex = e as DiscoreWebSocketException;
            if (dex != null)
            {
                GatewayDisconnectCode code = (GatewayDisconnectCode)dex.ErrorCode;
                switch (code)
                {
                    case GatewayDisconnectCode.InvalidShard:
                    case GatewayDisconnectCode.AuthenticationFailed:
                        // Not safe to reconnect
                        log.LogError($"[{code} ({(int)code})] Unsafe to continue, NOT reconnecting gateway.");
                        OnFatalDisconnection?.Invoke(this, code);
                        break;
                    default:
                        // Safe to reconnect
                        Reconnect();
                        break;
                }
            }
            else
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
