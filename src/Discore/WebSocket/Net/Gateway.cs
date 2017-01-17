using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    partial class Gateway : IDiscordGateway, IDisposable
    {
        public Shard Shard { get { return shard; } }
        public DiscoreWebSocketState SocketState { get { return socket.State; } }

        public event EventHandler OnReconnected;
        public event EventHandler<GatewayDisconnectCode> OnFatalDisconnection;

        /// <summary>
        /// Maximum number of missed heartbeats before timing out.
        /// </summary>
        const int HEARTBEAT_TIMEOUT_MISSED_PACKETS = 3;

        const int GATEWAY_VERSION = 5;

        DiscordWebSocketApplication app;
        Shard shard;

        CancellationTokenSource taskCancelTokenSource;

        DiscoreWebSocket socket;
        DiscoreLogger log;

        int sequence;
        string sessionId;
        int heartbeatInterval;
        int heartbeatTimeoutAt;

        Task heartbeatTask;

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
        async Task<string> GetGatewayUrlAsync(bool forceFindNew = false)
        {
            DiscoreLocalStorage localStorage = await DiscoreLocalStorage.GetInstanceAsync();

            string gatewayUrl = localStorage.GatewayUrl;
            if (forceFindNew || string.IsNullOrWhiteSpace(gatewayUrl))
            {
                gatewayUrl = await app.HttpApi.Gateway.Get();

                localStorage.GatewayUrl = gatewayUrl;
                await localStorage.SaveAsync();
            }

            return gatewayUrl;
        }

        /// <param name="gatewayResume">Will send a resume payload instead of an identify upon reconnecting when true.</param>
        /// <param name="waitForHeartbeatThread">Wether to wait for the heartbeat thread to end before reconnecting.</param>
        /// <exception cref="ObjectDisposedException">Thrown if this gateway connection has been disposed.</exception>
        public async Task<bool> ConnectAsync(bool gatewayResume = false, bool waitForHeartbeatThread = true)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(socket), "Cannot use a disposed gateway connection.");
            if (socket.State != DiscoreWebSocketState.Closed)
                throw new InvalidOperationException("Failed to connect, the Gateway is already connected or connecting.");

            // Reset gateway state
            if (!gatewayResume)
                Reset();

            string gatewayUrl = await GetGatewayUrlAsync();

            connectionRateLimiter.Invoke(); // Check with the connection rate limiter.

            bool connectedToSocket;

            try
            {
                // Attempt to connect to the WebSocket API.
                connectedToSocket = await socket.ConnectAsync($"{gatewayUrl}/?encoding=json&v={GATEWAY_VERSION}", 
                    CancellationToken.None);
            }
            catch
            {
                connectedToSocket = false;
            }

            if (connectedToSocket)
            {
                if (gatewayResume)
                    SendResumePayload();
                else
                    SendIdentifyPayload();

                int timeoutAt = Environment.TickCount + (10 * 1000); // Give Discord 10s to send Hello payload

                while (heartbeatInterval <= 0 && !TimeHelper.HasTickCountHit(timeoutAt))
                    Thread.Sleep(1);

                if (heartbeatInterval > 0)
                {
                    taskCancelTokenSource = new CancellationTokenSource();

                    // Handshake was successful, begin the heartbeat loop
                    heartbeatTask = new Task(HeartbeatLoop, taskCancelTokenSource.Token);
                    heartbeatTask.Start();

                    return true;
                }
                else
                    // We timed out, but the socket is still connected.
                    await socket.DisconnectAsync(CancellationToken.None);
            }
            else
            {
                // Since we failed to connect, try and find the new gateway url.
                string newGatewayUrl = await GetGatewayUrlAsync(true);
                if (gatewayUrl != newGatewayUrl)
                {
                    // If the endpoint did change, overwrite it in storage.
                    DiscoreLocalStorage localStorage = await DiscoreLocalStorage.GetInstanceAsync();
                    localStorage.GatewayUrl = newGatewayUrl;

                    await localStorage.SaveAsync();
                }
            }

            return false;
        }

        /// <exception cref="ObjectDisposedException">Thrown if this gateway connection has been disposed.</exception>
        public async Task DisconnectAsync()
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(socket), "Cannot use a disposed gateway connection.");

            // Disconnect the socket
            await socket.DisconnectAsync(CancellationToken.None);
            // Wait for heartbeat loop to finish
            await heartbeatTask;
        }

        void Reset()
        {
            sequence = 0;
            heartbeatInterval = 0;

            shard.User = null;
        }

        async void HeartbeatLoop()
        {
            // Set timeout
            heartbeatTimeoutAt = Environment.TickCount + (heartbeatInterval * HEARTBEAT_TIMEOUT_MISSED_PACKETS);

            bool timedOut = false;

            // Run heartbeat loop until socket is ended or timed out
            while (socket.State == DiscoreWebSocketState.Open)
            {
                if (TimeHelper.HasTickCountHit(heartbeatTimeoutAt))
                {
                    timedOut = true;
                    break;
                }

                SendHeartbeatPayload();

                await Task.Delay(heartbeatInterval, taskCancelTokenSource.Token);
            }

            // If we have timed out and the socket was not disconnected, attempt to reconnect.
            if (timedOut && socket.State == DiscoreWebSocketState.Open)
            {
                log.LogInfo("Connection timed out, reconnecting...");

                // Attempt reconnect
                await socket.DisconnectAsync(CancellationToken.None);
                await ReconnectAsync(false, false);

                // Once the socket reconnects, we can let the heartbeat thread
                // gracefully end, as it will be overwritten by the new handshake.
            }
        }

        /// <param name="gatewayResume">Whether to perform a full-reconnect or just a resume.</param>
        /// <param name="waitForHeartbeatThread">Wether to wait for the heartbeat thread to end before reconnecting.</param>
        async Task ReconnectAsync(bool gatewayResume = false, bool waitForHeartbeatThread = true)
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
                        if (await ConnectAsync(gatewayResume, waitForHeartbeatThread))
                            break;
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"[Reconnect] {ex}");
                    }
                }

                isReconnecting = false;
                OnReconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        private async void Socket_OnError(object sender, Exception e)
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
                        await ReconnectAsync();
                        break;
                }
            }
            else
                // Socket errors are fatal, so attempt to reconnect.
                await ReconnectAsync();
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

                socket?.Dispose();
                taskCancelTokenSource?.Dispose();
            }
        }
    }
}
