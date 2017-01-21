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
        Task reconnectTask;
        CancellationTokenSource reconnectCancelTokenSource;

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
        async Task<string> GetGatewayUrlAsync(CancellationToken cancellationToken, bool forceFindNew = false)
        {
            DiscoreLocalStorage localStorage = await DiscoreLocalStorage.GetInstanceAsync();

            string gatewayUrl = localStorage.GatewayUrl;
            if (forceFindNew || string.IsNullOrWhiteSpace(gatewayUrl))
            {
                gatewayUrl = await app.HttpApi.Gateway.Get(cancellationToken);

                localStorage.GatewayUrl = gatewayUrl;
                await localStorage.SaveAsync();
            }

            return gatewayUrl;
        }

        /// <param name="gatewayResume">Will send a resume payload instead of an identify upon reconnecting when true.</param>
        /// <exception cref="ObjectDisposedException">Thrown if this gateway connection has been disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown if already connected or currently connecting.</exception>
        /// <exception cref="TaskCanceledException"></exception>
        public async Task<bool> ConnectAsync(CancellationToken cancellationToken, bool gatewayResume = false)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(socket), "Cannot use a disposed gateway connection.");
            if (socket.State != DiscoreWebSocketState.Closed)
                throw new InvalidOperationException("Failed to connect, the Gateway is already connected or connecting.");

            // Reset gateway state only if not resuming
            if (!gatewayResume)
                Reset();

            // Get the gateway url
            string gatewayUrl = await GetGatewayUrlAsync(cancellationToken);

            // Check with the connection rate limiter.
            connectionRateLimiter.Invoke();

            // Attempt to connect to the WebSocket API.
            bool connectedToSocket;

            try
            {
                connectedToSocket = await socket.ConnectAsync($"{gatewayUrl}/?encoding=json&v={GATEWAY_VERSION}", cancellationToken);
            }
            catch
            {
                connectedToSocket = false;
            }

            if (connectedToSocket)
            {
                // Send resume or identify payload
                if (gatewayResume)
                    SendResumePayload();
                else
                    SendIdentifyPayload();

                // Give Discord 10s to send Hello payload
                int timeoutAt = Environment.TickCount + (10 * 1000); 

                while (heartbeatInterval <= 0 && !TimeHelper.HasTickCountHit(timeoutAt))
                    await Task.Delay(100, cancellationToken);

                if (heartbeatInterval > 0)
                {
                    taskCancelTokenSource = new CancellationTokenSource();

                    // Handshake was successful, begin the heartbeat loop
                    heartbeatTask = new Task(HeartbeatLoop, taskCancelTokenSource.Token);
                    heartbeatTask.Start();

                    return true;
                }
                else if (socket.State == DiscoreWebSocketState.Open)
                    // We timed out, but the socket is still connected.
                    await socket.DisconnectAsync(CancellationToken.None);
            }
            else
            {
                // Since we failed to connect, try and find the new gateway url.
                string newGatewayUrl = await GetGatewayUrlAsync(cancellationToken, true);
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
        /// <exception cref="TaskCanceledException"></exception>
        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(socket), "Cannot use a disposed gateway connection.");

            // Cancel reconnection
            await CancelReconnect();

            // Disconnect the socket
            if (socket.State == DiscoreWebSocketState.Open)
                await socket.DisconnectAsync(cancellationToken);

            // Wait for heartbeat loop to finish
            if (heartbeatTask != null)
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
            bool timedOut = false;

            try
            {
                // Set timeout
                heartbeatTimeoutAt = Environment.TickCount + (heartbeatInterval * HEARTBEAT_TIMEOUT_MISSED_PACKETS);

                // Run heartbeat loop until socket is ended or timed out
                while (socket.State == DiscoreWebSocketState.Open)
                {
                    if (TimeHelper.HasTickCountHit(heartbeatTimeoutAt))
                    {
                        timedOut = true;
                        break;
                    }

                    try
                    {
                        SendHeartbeatPayload();

                        await Task.Delay(heartbeatInterval, taskCancelTokenSource.Token);
                    }
                    // Two valid exceptions are a cancellation and a dispose before full-disconnect.
                    catch (TaskCanceledException) { }
                    catch (ObjectDisposedException) { }
                }
            }
            catch (Exception ex)
            {
                // Should never happen, but just incase.
                log.LogError($"[HeartbeatLoop] {ex}");

                // Start reconnecting
                log.LogInfo("Reconnecting from heartbeat exception...");
                BeginReconnect();
            }

            // If we have timed out and the socket was not disconnected, attempt to reconnect.
            if (timedOut && socket.State == DiscoreWebSocketState.Open && !isReconnecting && !isDisposed)
            {
                log.LogInfo("Connection timed out, reconnecting...");

                // Start reconnecting
                BeginReconnect();

                // Let this task end, as it will be overwritten once reconnection completes.
            }
        }

        /// <param name="gatewayResume">Whether to perform a full-reconnect or just a resume.</param>
        void BeginReconnect(bool gatewayResume = false)
        {
            // Since a reconnect can be started from multiple threads,
            // ensure that we do not enter this loop simultaneously.
            if (!isReconnecting)
            {
                reconnectCancelTokenSource = new CancellationTokenSource();

                isReconnecting = true;

                reconnectTask = new Task(ReconnectLoop, new Tuple<bool>(gatewayResume));
                reconnectTask.Start();
            }
        }

        async Task CancelReconnect()
        {
            if (isReconnecting)
            {
                reconnectCancelTokenSource.Cancel();
                await reconnectTask;
            }
        }

        async void ReconnectLoop(object _state)
        {
            Tuple<bool> state = (Tuple<bool>)_state;
            bool gatewayResume = state.Item1;

            // Make sure we disconnect first
            if (socket.State == DiscoreWebSocketState.Open)
                await socket.DisconnectAsync(reconnectCancelTokenSource.Token);

            // Let heartbeat task finish
            if (heartbeatTask != null && heartbeatTask.Status == TaskStatus.Running)
                await heartbeatTask;

            // Keep trying to connect until canceled
            while (!isDisposed && !reconnectCancelTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (await ConnectAsync(reconnectCancelTokenSource.Token, gatewayResume))
                        break;
                }
                catch (Exception ex)
                {
                    log.LogError($"[Reconnect] {ex}");
                }
            }

            if (!reconnectCancelTokenSource.IsCancellationRequested)
            {
                isReconnecting = false;

                if (!isDisposed)
                    OnReconnected?.Invoke(this, EventArgs.Empty);
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
                        BeginReconnect();
                        break;
                }
            }
            else
                // Socket errors are fatal, so attempt to reconnect.
                BeginReconnect();
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
                reconnectCancelTokenSource?.Dispose();
            }
        }
    }
}
