using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
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

        bool wasRateLimited;

        /// <summary>
        /// Will be true while ConnectAsync is running.
        /// </summary>
        bool isConnecting;

        bool isReconnecting;
        Task reconnectTask;
        CancellationTokenSource reconnectCancelTokenSource;

        DiscoreCache cache;

        GatewayRateLimiter identityRateLimiter;
        GatewayRateLimiter outboundEventRateLimiter;
        GatewayRateLimiter gameStatusUpdateRateLimiter;

        AsyncManualResetEvent helloPayloadEvent;

        internal Gateway(DiscordWebSocketApplication app, Shard shard)
        {
            this.app = app;
            this.shard = shard;

            cache = shard.Cache;

            string logName = $"Gateway#{shard.Id}";
               
            log = new DiscoreLogger(logName);

            helloPayloadEvent = new AsyncManualResetEvent();

            // Up-to-date rate limit parameters: https://discordapp.com/developers/docs/topics/gateway#rate-limiting
            identityRateLimiter = new GatewayRateLimiter(5, 1); // One identity packet per 5 seconds
            outboundEventRateLimiter = new GatewayRateLimiter(60, 120); // 120 outbound events every 60 seconds
            gameStatusUpdateRateLimiter = new GatewayRateLimiter(60, 5); // 5 status updates per minute

            InitializePayloadHandlers();
            InitializeDispatchHandlers();
            
            socket = new DiscoreWebSocket(WebSocketDataType.Json, logName);
            socket.OnError += Socket_OnError;
            socket.OnMessageReceived += Socket_OnMessageReceived;
        }

        /// <param name="forceFindNew">Whether to call the HTTP forcefully, or use the local cached value.</param>
        async Task<string> GetGatewayUrlAsync(CancellationToken cancellationToken, bool forceFindNew = false)
        {
            DiscoreLocalStorage localStorage = await DiscoreLocalStorage.GetInstanceAsync().ConfigureAwait(false);

            string gatewayUrl = localStorage.GatewayUrl;
            if (forceFindNew || string.IsNullOrWhiteSpace(gatewayUrl))
            {
                gatewayUrl = await app.HttpApi.Gateway.Get(cancellationToken).ConfigureAwait(false);

                localStorage.GatewayUrl = gatewayUrl;
                await localStorage.SaveAsync().ConfigureAwait(false);
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

            isConnecting = true;

            try
            {
                log.LogVerbose($"[ConnectAsync] Attempting to connect - gatewayResume: {gatewayResume}");

                // Reset gateway state only if not resuming
                if (!gatewayResume)
                    Reset();

                // Get the gateway url
                string gatewayUrl = await GetGatewayUrlAsync(cancellationToken).ConfigureAwait(false);

                log.LogVerbose($"[ConnectAsync] gatewayUrl: {gatewayUrl}");

                // If was rate limited from last disconnect, wait extra time.
                if (wasRateLimited)
                {
                    wasRateLimited = false;
                    await Task.Delay(identityRateLimiter.ResetTimeSeconds * 1000).ConfigureAwait(false);
                }

                if (!gatewayResume)
                {
                    // Check with the identity rate limiter.
                    await identityRateLimiter.Invoke().ConfigureAwait(false);
                }

                // Reset the hello event so we know when the connection was successful.
                helloPayloadEvent.Reset();

                // Attempt to connect to the WebSocket API.
                bool connectedToSocket;

                try
                {
                    connectedToSocket = await socket.ConnectAsync($"{gatewayUrl}/?encoding=json&v={GATEWAY_VERSION}", cancellationToken)
                        .ConfigureAwait(false);
                }
                catch
                {
                    connectedToSocket = false;
                }

                if (connectedToSocket)
                {
                    log.LogVerbose("[ConnectAsync] Awaiting hello...");

                    // Give Discord 10s to send Hello payload
                    const int helloTimeout = 10 * 1000;
                    Task helloWaitTask = await Task.WhenAny(helloPayloadEvent.WaitAsync(cancellationToken), Task.Delay(helloTimeout, cancellationToken))
                        .ConfigureAwait(false);

                    if (helloWaitTask.IsCanceled)
                        throw new TaskCanceledException(helloWaitTask);

                    // Check if the payload was recieved or if we timed out.
                    if (heartbeatInterval > 0)
                    {
                        taskCancelTokenSource = new CancellationTokenSource();

                        // Handshake was successful, begin the heartbeat loop
                        heartbeatTask = HeartbeatLoop();

                        // Send resume or identify payload
                        if (gatewayResume)
                            await SendResumePayload().ConfigureAwait(false);
                        else
                            await SendIdentifyPayload().ConfigureAwait(false);

                        log.LogVerbose("[ConnectAsync] Connection successful.");
                        return true;
                    }
                    else if (socket.State == DiscoreWebSocketState.Open)
                    {
                        log.LogError("[ConnectAsync] Timed out waiting for hello.");

                        // We timed out, but the socket is still connected.
                        await socket.DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
                    }
                }
                else
                {
                    // Since we failed to connect, try and find the new gateway url.
                    string newGatewayUrl = await GetGatewayUrlAsync(cancellationToken, true).ConfigureAwait(false);
                    if (gatewayUrl != newGatewayUrl)
                    {
                        // If the endpoint did change, overwrite it in storage.
                        DiscoreLocalStorage localStorage = await DiscoreLocalStorage.GetInstanceAsync().ConfigureAwait(false);
                        localStorage.GatewayUrl = newGatewayUrl;

                        await localStorage.SaveAsync().ConfigureAwait(false);
                    }
                }

                log.LogError("[ConnectAsync] Failed to connect.");
                return false;
            }
            finally
            {
                isConnecting = false;
            }
        }

        /// <exception cref="ObjectDisposedException">Thrown if this gateway connection has been disposed.</exception>
        /// <exception cref="TaskCanceledException"></exception>
        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(socket), "Cannot use a disposed gateway connection.");

            log.LogVerbose("[DisconnectAsync] Disconnecting...");

            // Cancel reconnection
            await CancelReconnectLoop().ConfigureAwait(false);

            // Disconnect the socket
            if (socket.State == DiscoreWebSocketState.Open)
                await socket.DisconnectAsync(cancellationToken).ConfigureAwait(false);

            log.LogVerbose("[DisconnectAsync] Socket disconnected...");

            taskCancelTokenSource.Cancel();

            // Wait for heartbeat loop to finish
            if (heartbeatTask != null)
                await heartbeatTask.ConfigureAwait(false);

            log.LogVerbose("[DisconnectAsync] Disconnection successful.");
        }

        void Reset()
        {
            sequence = 0;
            heartbeatInterval = 0;
            sessionId = null;

            shard.User = null;
        }

        async Task HeartbeatLoop()
        {
            bool timedOut = false;

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
                    await SendHeartbeatPayload().ConfigureAwait(false);

                    await Task.Delay(heartbeatInterval, taskCancelTokenSource.Token).ConfigureAwait(false);
                }
                // Two valid exceptions are a cancellation and a dispose before full-disconnect.
                catch (TaskCanceledException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex)
                {
                    // Should never happen and is not considered fatal, but log it just in case.
                    log.LogError($"[HeartbeatLoop] {ex}");
                }
            }

            // If we have timed out and the socket was not disconnected, attempt to reconnect.
            if (timedOut && socket.State == DiscoreWebSocketState.Open && !isReconnecting && !isDisposed)
            {
                log.LogInfo("[HeartbeatLoop] Connection timed out.");

                // Start resuming...
                BeginResume();

                // Let this task end, as it will be overwritten once reconnection completes.
            }
        }

        void BeginNewSession(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            if (!isReconnecting && !isConnecting)
            {
                isReconnecting = true;
                reconnectCancelTokenSource = new CancellationTokenSource();

                reconnectTask = ReconnectLoop(false, closeStatus);
            }
        }

        // Defaults to an empty close status because a 1000 normal closure code would
        // end up starting a new session on Discord's end.
        void BeginResume(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.InternalServerError)
        {
            if (!isReconnecting && !isConnecting)
            {
                isReconnecting = true;
                reconnectCancelTokenSource = new CancellationTokenSource();

                reconnectTask = ReconnectLoop(true, closeStatus);
            }
        }

        async Task CancelReconnectLoop()
        {
            if (isReconnecting)
            {
                reconnectCancelTokenSource.Cancel();
                await reconnectTask.ConfigureAwait(false);
            }
        }

        async Task ReconnectLoop(bool gatewayResume, WebSocketCloseStatus closeStatus)
        {
            if (gatewayResume)
                log.LogVerbose("ReconnectLoop] Beginning resume...");
            else
                log.LogVerbose("ReconnectLoop] Beginning new session...");

            // Disable socket error handling until we have reconnected.
            // This avoids the socket performing its own disconnection
            // procedure from an error, which may occur while we reconnect,
            // especially if this reconnection originated from a timeout.
            socket.IgnoreSocketErrors = true;

            // Make sure we disconnect first
            if (socket.State == DiscoreWebSocketState.Open)
                await socket.DisconnectAsync(reconnectCancelTokenSource.Token, closeStatus)
                    .ConfigureAwait(false);

            log.LogVerbose("[ReconnectLoop] Socket disconnected...");

            // Let heartbeat task finish
            if (heartbeatTask != null && heartbeatTask.Status == TaskStatus.Running)
                await heartbeatTask.ConfigureAwait(false);

            log.LogVerbose("[ReconnectLoop] Heartbeat loop completed, attempting to reconnect...");

            // Keep trying to connect until canceled
            while (!isDisposed && !reconnectCancelTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (await ConnectAsync(reconnectCancelTokenSource.Token, gatewayResume).ConfigureAwait(false))
                        break;
                }
                catch (Exception ex)
                {
                    log.LogError($"[ReconnectLoop] {ex}");
                }

                log.LogVerbose("[ReconnectLoop] Waiting 5s before retrying...");
                await Task.Delay(5000);
            }

            // Restore socket errors regardless of cancellation or success.
            socket.IgnoreSocketErrors = false;

            if (!reconnectCancelTokenSource.IsCancellationRequested)
            {
                if (gatewayResume)
                    log.LogInfo("[ReconnectLoop] Resume completed.");
                else
                    log.LogInfo("[ReconnectLoop] New session successful.");

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
                    case GatewayDisconnectCode.ShardingRequired:
                        // Not safe to reconnect
                        log.LogError($"[{code} ({(int)code})] Unsafe to continue, NOT reconnecting gateway.");
                        OnFatalDisconnection?.Invoke(this, code);
                        break;
                    case GatewayDisconnectCode.InvalidSeq:
                    case GatewayDisconnectCode.SessionTimeout:
                    case GatewayDisconnectCode.UnknownError:
                        // Safe to reconnect, but needs a new session.
                        BeginNewSession();
                        break;
                    case GatewayDisconnectCode.NotAuthenticated:
                        // This really should never happen, but will require a new session.
                        log.LogWarning("Sent gateway payload before we identified!");
                        BeginNewSession();
                        break;
                    case GatewayDisconnectCode.RateLimited:
                        // Doesn't require a new session, but we need to wait a bit.
                        log.LogWarning("Gateway is being rate limited!");
                        wasRateLimited = true;
                        BeginResume();
                        break;
                    default:
                        // Safe to just resume
                        BeginResume();
                        break;
                }
            }
            else
                // Just an error on our end, go ahead and resume.
                BeginResume(WebSocketCloseStatus.InternalServerError);
        }

        private async void Socket_OnMessageReceived(object sender, DiscordApiData e)
        {
            try
            {
                GatewayOPCode op = (GatewayOPCode)e.GetInteger("op");
                DiscordApiData data = e.Get("d");

                PayloadCallback callback;
                if (payloadHandlers.TryGetValue(op, out callback))
                {
                    if (callback.Synchronous != null)
                        callback.Synchronous(e, data);
                    else
                        await callback.Asynchronous(e, data);
                }
                else
                    log.LogWarning($"Missing handler for payload: {op}({(int)op})");
            }
            catch (Exception ex)
            {
                log.LogError("[OnMessageReceived] Unhandled Exception:");
                log.LogError(ex);
            }
        }

        /// <summary>
        /// Logs the _trace field in the given event data if present.
        /// Used by payload and dispatch event handlers.
        /// </summary>
        void LogServerTrace(string prefix, DiscordApiData data)
        {
            IList<DiscordApiData> traceArray = data.GetArray("_trace");
            if (traceArray != null)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < traceArray.Count; i++)
                {
                    if (i > 0)
                        sb.Append(", ");

                    sb.Append(traceArray[i].ToString());
                }

                log.LogVerbose($"[{prefix}] trace = {sb}");
            }
        }

        /// <summary>
        /// Returns each private method with the given attribute in this class.
        /// </summary>
        IEnumerable<Tuple<MethodInfo, T>> GetMethodsWithAttribute<T>()
            where T : Attribute
        {
            Type gatewayType = typeof(Gateway);

            foreach (MethodInfo method in gatewayType.GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                T attr = method.GetCustomAttribute<T>();
                if (attr != null)
                    yield return new Tuple<MethodInfo, T>(method, attr);
            }
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
