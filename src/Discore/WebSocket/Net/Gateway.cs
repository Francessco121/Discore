using Discore.Http;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    partial class Gateway : IDiscordGateway, IDisposable
    {
        enum GatewayState
        {
            Disconnected,
            Connecting,
            Connected
        }

        public Shard Shard => shard;

        public event EventHandler OnReconnected;
        public event EventHandler<GatewayCloseCode> OnFatalDisconnection;

        const int GATEWAY_VERSION = 5;

        DiscordWebSocketApplication app;
        Shard shard;
        DiscoreCache cache;

        ShardStartConfig lastShardStartConfig;

        GatewaySocket socket;

        GatewayRateLimiter connectionRateLimiter;
        // These two rate limiters are used by the socket itself,
        // but must be saved between creating new sockets.
        GatewayRateLimiter outboundPayloadRateLimiter;
        GatewayRateLimiter gameStatusUpdateRateLimiter;

        /// <summary>
        /// State to be tracked only for the public API of this class.
        /// This does not represent the state of the underlying socket.
        /// </summary>
        GatewayState state;

        Task connectTask;
        /// <summary>
        /// Whether the next HELLO payload should be responded to with a RESUME, otherwise IDENTIFY.
        /// </summary>
        bool isConnectionResuming;
        /// <summary>
        /// Used to cancel the connect task when it is started automatically (i.e. not from public ConnectAsync).
        /// </summary>
        CancellationTokenSource connectTaskCancellationSource;

        AsyncManualResetEvent gatewayReadyEvent;
        /// <summary>
        /// Used to cancel operations that wait for the gatewayReadyEvent.
        /// Cancellation occurs when the Gateway is disconnected publicly (i.e. not from a socket error).
        /// </summary>
        CancellationTokenSource gatewayReadyEventCancellationSource;

        DiscoreLogger log;

        int lastSequence;
        string sessionId;

        bool isDisposed;

        internal Gateway(DiscordWebSocketApplication app, Shard shard)
        {
            this.app = app;
            this.shard = shard;

            cache = shard.Cache;

            log = new DiscoreLogger($"Gateway#{shard.Id}");
            state = GatewayState.Disconnected;

            gatewayReadyEvent = new AsyncManualResetEvent();
            gatewayReadyEventCancellationSource = new CancellationTokenSource();

            // Up-to-date rate limit parameters: https://discordapp.com/developers/docs/topics/gateway#rate-limiting
            connectionRateLimiter = new GatewayRateLimiter(5, 1); // 1 connection attempt per 5 seconds
            outboundPayloadRateLimiter = new GatewayRateLimiter(60, 120); // 120 outbound payloads every 60 seconds
            gameStatusUpdateRateLimiter = new GatewayRateLimiter(60, 5); // 5 status updates per minute

            InitializeDispatchHandlers();
        }

        #region Deprecated Public API
        [Obsolete]
        public void UpdateStatus(string game = null, int? idleSince = default(int?))
            => UpdateStatusAsync(game, idleSince).Wait();

        [Obsolete]
        public void RequestGuildMembers(Action<IReadOnlyList<DiscordGuildMember>> callback, Snowflake guildId,
            string query = "", int limit = 0)
            => RequestGuildMembersAsync(callback, guildId, query, limit).Wait();

        /// <exception cref="InvalidOperationException">Thrown if this method is used before the Gateway is connected.</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="OperationCanceledException">Thrown if the gateway connection is closed while sending.</exception>
        [Obsolete]
        public async Task RequestGuildMembersAsync(Action<IReadOnlyList<DiscordGuildMember>> callback, Snowflake guildId,
            string query = "", int limit = 0)
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (state != GatewayState.Connected)
                throw new InvalidOperationException("The gateway is not currently connected!");

            // Create GUILD_MEMBERS_CHUNK event handler
            EventHandler<GuildMemberChunkEventArgs> eventHandler = null;
            eventHandler = (sender, args) =>
            {
                // Unhook event handler
                OnGuildMembersChunk -= eventHandler;

                // Return members
                callback(args.Members);
            };

            // Hook in event handler
            OnGuildMembersChunk += eventHandler;

            try
            {
                await RepeatTrySendPayload(CancellationToken.None, "RequestGuildMembers (obselete)", async () =>
                {
                    // Try to request guild members
                    await socket.SendRequestGuildMembersPayload(guildId, query, limit).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            finally
            {
                OnGuildMembersChunk -= eventHandler;
            }
        }
        #endregion

        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the cancellation token is cancelled or the gateway connection is closed while sending.
        /// </exception>
        public async Task UpdateStatusAsync(string game = null, int? idleSince = null, CancellationToken? cancellationToken = null)
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (state != GatewayState.Connected)
                throw new InvalidOperationException("The gateway is not currently connected!");

            CancellationToken ct = cancellationToken ?? CancellationToken.None;

            await RepeatTrySendPayload(ct, "UpdateStatus", async () =>
            {
                // Try to send the status update
                await socket.SendStatusUpdate(game, idleSince).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the cancellation token is cancelled or the gateway connection is closed while sending.
        /// </exception>
        public async Task RequestGuildMembersAsync(Snowflake guildId, string query = "", int limit = 0, 
            CancellationToken? cancellationToken = null)
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (state != GatewayState.Connected)
                throw new InvalidOperationException("The gateway is not currently connected!");

            CancellationToken ct = cancellationToken ?? CancellationToken.None;

            await RepeatTrySendPayload(ct, "RequestGuildMembers", async () =>
            {
                // Try to request guild members
                await socket.SendRequestGuildMembersPayload(guildId, query, limit).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        /// <exception cref="OperationCanceledException">Thrown if the gateway connection is closed while sending.</exception>
        internal async Task SendVoiceStateUpdatePayload(Snowflake guildId, Snowflake? channelId, bool isMute, bool isDeaf,
            CancellationToken cancellationToken)
        {
            await RepeatTrySendPayload(cancellationToken, "RequestGuildMembers", async () =>
            {
                // Try to send the status update
                await socket.SendVoiceStateUpdatePayload(guildId, channelId, isMute, isDeaf).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Continuously retries to call the specified callback (which should only be a payload send).
        /// <para>
        /// Retries if the callback throws a InvalidOperationException or DiscordWebSocketException.
        /// Also waits for the gateway connection to be ready before calling the callback.
        /// </para>
        /// </summary>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the cancellation token is cancelled or the gateway connection is closed while sending.
        /// </exception>
        async Task RepeatTrySendPayload(CancellationToken ct, string opName, Func<Task> callback)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            // This can be cancelled either by the caller, or the gateway disconnecting.
            using (ct.Register(() => cts.Cancel()))
            using (gatewayReadyEventCancellationSource.Token.Register(() => cts.Cancel()))
            {
                while (true)
                {
                    cts.Token.ThrowIfCancellationRequested();

                    if (state != GatewayState.Connected)
                        // Cancel if the gateway connection is closed from the outside.
                        throw new OperationCanceledException("The gateway connection was closed.");

                    bool waitingForReady = false;
                    if (!gatewayReadyEvent.IsSet)
                    {
                        waitingForReady = true;
                        log.LogVerbose($"[{opName}:RepeatTrySendPayload] Awaiting gateway ready...");
                    }

                    // Wait until the gateway connection is ready
                    await gatewayReadyEvent.WaitAsync(cts.Token).ConfigureAwait(false);

                    if (waitingForReady)
                        log.LogVerbose($"[{opName}:RepeatTrySendPayload] Gateway is ready.");

                    try
                    {
                        // Try the callback
                        await callback().ConfigureAwait(false);

                        // Call succeeded
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        log.LogVerbose($"[{opName}:RepeatTrySendPayload] InvalidOperationException, retrying...");

                        // The socket was closed between waiting for the socket to open
                        // and sending the payload. Shouldn't ever happen, give the socket
                        // some time to flip back to disconnected.
                        await Task.Delay(500, cts.Token).ConfigureAwait(false);
                    }
                    catch (DiscordWebSocketException dwex)
                    {
                        log.LogVerbose($"[{opName}:RepeatTrySendPayload] DiscordWebSocketException: " +
                            $"{dwex.Error} = {dwex.Message}, retrying...");

                        // Payload failed to send because the socket blew up,
                        // just retry after giving the socket some time to flip to
                        // a disconencted state.
                        await Task.Delay(500, cts.Token).ConfigureAwait(false);
                    }
                }
            }
        }

        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public Task ConnectAsync(ShardStartConfig config, CancellationToken cancellationToken)
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (state == GatewayState.Connected)
                throw new InvalidOperationException("The gateway is already connected!");
            if (state == GatewayState.Connecting)
                throw new InvalidOperationException("The gateway is already connecting!");

            // Begin connecting
            state = GatewayState.Connecting;
            lastShardStartConfig = config;
            connectTask = ConnectLoop(false, cancellationToken);

            // Register a continue with so we can set the state appropriately once
            // the connection is finished.
            connectTask.ContinueWith(task =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    // Connection successful
                    log.LogVerbose("[ConnectAsync] Setting state to Connected.");
                    state = GatewayState.Connected;
                }
                else
                {
                    // Reset to disconnected if cancelled or failed
                    log.LogVerbose("[ConnectAsync] Setting state to Disconnected.");
                    state = GatewayState.Disconnected;
                }
            });

            return connectTask;
        }

        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task DisconnectAsync()
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (state != GatewayState.Connected)
                throw new InvalidOperationException("The gateway is not connected!");

            log.LogVerbose("Disconnecting...");
            state = GatewayState.Disconnected;

            gatewayReadyEventCancellationSource.Cancel();

            if (connectTask != null)
            {
                // Cancel any automatic reconnection
                connectTaskCancellationSource?.Cancel();

                // Wait for the automatic reconnection to end
                try
                {
                    await connectTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException) { /* Expected to happen. */ }
                catch (Exception ex)
                {
                    // Should never happen, but there isn't anything we can do here.
                    log.LogError($"[DisconnectAsync] Uncaught exception found in connect task: {ex}");
                }
            }

            // Disconnect the socket if needed
            if (socket.CanBeDisconnected)
                await socket.DisconnectAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting...", CancellationToken.None)
                    .ConfigureAwait(false);

            log.LogInfo("Disconnected.");
        }

        async Task<string> GetGatewayUrlAsync()
        {
            DiscoreLocalStorage localStorage = await DiscoreLocalStorage.GetInstanceAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(localStorage.GatewayUrl))
            {
                try
                {
                    await UpdateGatewayUrlAsync().ConfigureAwait(false);
                }
                catch (DiscordHttpApiException httpEx)
                {
                    // Application most likely has no connection to the Discord API,
                    // just return what we have in storage and let the connection loop
                    // try again.

                    log.LogError($"[GetGatewayUrlAsync] Failed to update gateway url: {httpEx}");
                }
            }

            return localStorage.GatewayUrl ?? "";
        }

        async Task UpdateGatewayUrlAsync()
        {
            DiscoreLocalStorage localStorage = await DiscoreLocalStorage.GetInstanceAsync().ConfigureAwait(false);

            string gatewayUrl = await app.HttpApi.Gateway.Get().ConfigureAwait(false);

            if (localStorage.GatewayUrl != gatewayUrl)
            {
                localStorage.GatewayUrl = gatewayUrl;
                await localStorage.SaveAsync().ConfigureAwait(false);
            }
        }

        /// <exception cref="OperationCanceledException"></exception>
        async Task ConnectLoop(bool resume, CancellationToken cancellationToken)
        {
            // Keep track of whether this is a resume or new session so
            // we can respond to the HELLO payload appropriately.
            isConnectionResuming = resume;

            log.LogVerbose($"[ConnectLoop] resume = {resume}");

            gatewayReadyEvent.Reset();
            gatewayReadyEventCancellationSource = new CancellationTokenSource();

            while (!cancellationToken.IsCancellationRequested)
            {
                // Ensure previous socket has been closed
                if (socket != null)
                {
                    UnsubscribeSocketEvents();

                    if (resume)
                    {
                        // Store previous sequence
                        lastSequence = socket.Sequence;
                    }

                    if (socket.CanBeDisconnected)
                    {
                        log.LogVerbose($"[ConnectLoop] Disconnecting previous socket...");

                        // If for some reason the socket cannot be disconnected gracefully,
                        // DisconnectAsync will abort the socket after 5s.

                        if (resume)
                        {
                            // Make sure to disconnect with a non 1000 code to ensure Discord doesn't
                            // force us to make a new session since we are resuming.
                            await socket.DisconnectAsync(DiscordClientWebSocket.INTERNAL_CLIENT_ERROR, 
                                "Reconnecting to resume...", cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            await socket.DisconnectAsync(WebSocketCloseStatus.NormalClosure, 
                                "Starting new session...", cancellationToken).ConfigureAwait(false);
                        }
                    }

                    socket.Dispose();
                }

                if (!resume)
                {
                    // If not resuming, reset gateway session state.
                    lastSequence = 0;
                }

                // Create a new socket
                socket = new GatewaySocket($"GatewaySocket#{shard.Id}", lastSequence,
                    outboundPayloadRateLimiter, gameStatusUpdateRateLimiter);

                SubscribeSocketEvents();

                // Get the gateway URL
                string gatewayUrl = await GetGatewayUrlAsync().ConfigureAwait(false);
                log.LogVerbose($"[ConnectLoop] gatewayUrl = {gatewayUrl}");

                // Ensure we have a URL to the gateway
                if (string.IsNullOrWhiteSpace(gatewayUrl))
                {
                    log.LogError($"[ConnectLoop] No gateway URL to connect with, trying again in 5s...");
                    await Task.Delay(5000, cancellationToken).ConfigureAwait(false);

                    continue;
                }

                // Make sure we dont try and connect too often
                await connectionRateLimiter.Invoke().ConfigureAwait(false);

                try
                {
                    // Attempt to connect
                    await socket.ConnectAsync(new Uri($"{gatewayUrl}?v={GATEWAY_VERSION}&encoding=json"), cancellationToken)
                        .ConfigureAwait(false);

                    // At this point the socket has successfully connected
                    log.LogVerbose($"[ConnectLoop] Socket connected successfully.");
                    break;
                }
                catch (WebSocketException wsex)
                {
                    UnsubscribeSocketEvents();

                    // Failed to connect
                    log.LogError("[ConnectLoop] Failed to connect: " +
                        $"{wsex.WebSocketErrorCode} ({(int)wsex.WebSocketErrorCode}), {wsex.Message}");

                    // Try to update the gateway URL since we failed to connect the socket
                    try
                    {
                        await UpdateGatewayUrlAsync().ConfigureAwait(false);
                    }
                    catch (DiscordHttpApiException httpEx)
                    {
                        // Application most likely has no connection to the Discord API,
                        // just let the connection loop try again.

                        log.LogError($"[ConnectLoop] Failed to update gateway url: {httpEx}");
                    }

                    // Wait 5s then retry
                    log.LogVerbose("[ConnectLoop] Waiting 5s before retrying...");
                    await Task.Delay(5000, cancellationToken).ConfigureAwait(false);
                }
            }

            // If the token is cancelled between the socket successfully connecting and the loop exiting,
            // do not throw an exception because the connection did technically complete before the cancel.
            if (socket == null || !socket.IsConnected)
            {
                // If the loop stopped from the token being cancelled, ensure an exception is still thrown.
                cancellationToken.ThrowIfCancellationRequested();
            }

            // If this is an automatic reconnection, fire OnReconnected event
            if (state == GatewayState.Connected)
                OnReconnected?.Invoke(this, EventArgs.Empty);
        }

        void SubscribeSocketEvents()
        {
            socket.OnHello += Socket_OnHello;
            socket.OnRateLimited += Socket_OnRateLimited;
            socket.OnReconnectionRequired += Socket_OnReconnectionRequired;
            socket.OnFatalDisconnection += Socket_OnFatalDisconnection;
            socket.OnDispatch += Socket_OnDispatch;
        }

        void UnsubscribeSocketEvents()
        {
            socket.OnHello -= Socket_OnHello;
            socket.OnRateLimited -= Socket_OnRateLimited;
            socket.OnReconnectionRequired -= Socket_OnReconnectionRequired;
            socket.OnFatalDisconnection -= Socket_OnFatalDisconnection;
            socket.OnDispatch -= Socket_OnDispatch;
        }

        private void Socket_OnHello(object sender, EventArgs e)
        {
            if (isDisposed)
                return;

            if (isConnectionResuming)
                // Resume
                socket.SendResumePayload(app.Authenticator.GetToken(), sessionId, lastSequence);
            else
                // Identify
                socket.SendIdentifyPayload(app.Authenticator.GetToken(), 
                    lastShardStartConfig.GatewayLargeThreshold, shard.Id, app.ShardManager.TotalShardCount);
        }

        private void Socket_OnRateLimited(object sender, EventArgs e)
        {
            if (isDisposed)
                return;

            log.LogError("Gateway connection was rate limited!!");
        } 

        private void Socket_OnFatalDisconnection(object sender, GatewayCloseCode e)
        {
            if (isDisposed)
                return;

            log.LogVerbose("Fatal disconnection occured, setting state to Disconnected.");
            state = GatewayState.Disconnected;

            gatewayReadyEvent.Reset();

            OnFatalDisconnection?.Invoke(this, e);
        }

        void Socket_OnReconnectionRequired(object sender, bool requiresNewSession)
        {
            if (isDisposed)
                return;

            if (connectTask == null || connectTask.IsCompleted)
            {
                gatewayReadyEvent.Reset();

                log.LogVerbose("Beginning automatic reconnection...");
                connectTaskCancellationSource = new CancellationTokenSource();
                connectTask = ConnectLoop(!requiresNewSession, connectTaskCancellationSource.Token);
            }
        }

        private async void Socket_OnDispatch(object sender, DispatchEventArgs e)
        {
            if (isDisposed)
                return;

            string eventName = e.EventName;

            DispatchCallback callback;
            if (dispatchHandlers.TryGetValue(eventName, out callback))
            {
                try
                {
                    if (callback.Synchronous != null)
                        callback.Synchronous(e.Data);
                    else
                        await callback.Asynchronous(e.Data).ConfigureAwait(false);
                }
                catch (DiscoreCacheException cex)
                {
                    log.LogWarning($"[{eventName}] Did not complete because: {cex.Message}.");
                }
                catch (Exception ex)
                {
                    log.LogError($"[{eventName}] Unhandled exception: {ex}");
                }
            }
            else
                log.LogWarning($"Missing handler for dispatch event: {eventName}");
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                state = GatewayState.Disconnected;

                connectTaskCancellationSource?.Dispose();
                gatewayReadyEventCancellationSource?.Dispose();
                socket?.Dispose();
            }
        }
    }
}
