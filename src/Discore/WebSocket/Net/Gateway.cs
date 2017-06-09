using Discore.Http;
using Nito.AsyncEx;
using System;
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

        public event EventHandler<GatewayFailureData> OnFailure;

        const int GATEWAY_VERSION = 5;

        string botToken;
        DiscordHttpClient http;

        Shard shard;
        int totalShards;

        DiscordShardCache cache;

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

        AsyncManualResetEvent handshakeCompleteEvent;
        /// <summary>
        /// Used to cancel operations that wait for the handshakeCompleteEvent.
        /// Cancellation occurs when the Gateway is disconnected publicly (i.e. not from a socket error).
        /// </summary>
        CancellationTokenSource handshakeCompleteCancellationSource;

        GatewayFailureData gatewayFailureData;

        /// <summary>
        /// Milliseconds to wait before attempting the next socket connection. Will be reset after wait completes.
        /// </summary>
        int nextConnectionDelayMs;

        DiscoreLogger log;

        int lastSequence;
        string sessionId;

        bool isDisposed;

        internal Gateway(string botToken, Shard shard, int totalShards)
        {
            this.botToken = botToken;
            this.shard = shard;
            this.totalShards = totalShards;

            http = new DiscordHttpClient(botToken);

            cache = shard.Cache;

            log = new DiscoreLogger($"Gateway#{shard.Id}");
            state = GatewayState.Disconnected;

            handshakeCompleteEvent = new AsyncManualResetEvent();
            handshakeCompleteCancellationSource = new CancellationTokenSource();

            // Up-to-date rate limit parameters: https://discordapp.com/developers/docs/topics/gateway#rate-limiting
            connectionRateLimiter = new GatewayRateLimiter(5, 1); // 1 connection attempt per 5 seconds
            outboundPayloadRateLimiter = new GatewayRateLimiter(60, 120); // 120 outbound payloads every 60 seconds
            gameStatusUpdateRateLimiter = new GatewayRateLimiter(60, 5); // 5 status updates per minute

            InitializeDispatchHandlers();
        }

        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the cancellation token is cancelled or the gateway connection is closed while sending.
        /// </exception>
        public async Task UpdateStatusAsync(string game = null, int? idleSince = null, 
            CancellationToken? cancellationToken = null)
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
            using (handshakeCompleteCancellationSource.Token.Register(() => cts.Cancel()))
            {
                while (true)
                {
                    cts.Token.ThrowIfCancellationRequested();

                    if (state != GatewayState.Connected)
                        // Cancel if the gateway connection is closed from the outside.
                        throw new OperationCanceledException("The gateway connection was closed.");

                    bool waitingForReady = false;
                    if (!handshakeCompleteEvent.IsSet)
                    {
                        waitingForReady = true;
                        log.LogVerbose($"[{opName}:RepeatTrySendPayload] Awaiting completed gateway handshake...");
                    }

                    // Wait until the gateway connection is ready
                    await handshakeCompleteEvent.WaitAsync(cts.Token).ConfigureAwait(false);

                    if (waitingForReady)
                        log.LogVerbose($"[{opName}:RepeatTrySendPayload] Gateway is now fully connected after waiting.");

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

        /// <exception cref="GatewayHandshakeException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public async Task ConnectAsync(ShardStartConfig config, CancellationToken cancellationToken)
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

            gatewayFailureData = null;
            handshakeCompleteEvent.Reset();

            connectTask = ConnectLoop(false, cancellationToken);

            try
            {
                // Connect socket
                await connectTask.ConfigureAwait(false);

                try
                {
                    // Wait for the handshake to complete
                    await handshakeCompleteEvent.WaitAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Since this was cancelled after the socket connected,
                    // we need to do a full disconnect.
                    await FullDisconnect();
                    throw;
                }

                // Check for errors
                if (gatewayFailureData != null)
                    throw new GatewayHandshakeException(gatewayFailureData);

                // Connection successful
                log.LogVerbose("[ConnectAsync] Setting state to Connected.");
                state = GatewayState.Connected;
            }
            catch
            {
                // Reset to disconnected if cancelled or failed
                log.LogVerbose("[ConnectAsync] Setting state to Disconnected.");
                state = GatewayState.Disconnected;

                throw;
            }
        }

        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public Task DisconnectAsync()
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (state != GatewayState.Connected)
                throw new InvalidOperationException("The gateway is not connected!");

            return FullDisconnect();
        }

        /// <summary>
        /// Warning: Do not call from the context of the connect loop! A deadlock will occur!
        /// </summary>
        async Task FullDisconnect()
        {
            log.LogVerbose("Disconnecting...");
            state = GatewayState.Disconnected;

            handshakeCompleteCancellationSource.Cancel();

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

            log.LogVerbose("Disconnected.");
        }

        /// <exception cref="OperationCanceledException"></exception>
        async Task ConnectLoop(bool resume, CancellationToken cancellationToken)
        {
            // Keep track of whether this is a resume or new session so
            // we can respond to the HELLO payload appropriately.
            isConnectionResuming = resume;

            log.LogVerbose($"[ConnectLoop] resume = {resume}");

            handshakeCompleteEvent.Reset();
            handshakeCompleteCancellationSource = new CancellationTokenSource();

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
                        log.LogVerbose("[ConnectLoop] Disconnecting previous socket...");

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
                DiscoreLocalStorage localStorage;
                string gatewayUrl;
                try
                {
                    localStorage = await DiscoreLocalStorage.GetInstanceAsync().ConfigureAwait(false);
                    gatewayUrl = await localStorage.GetGatewayUrlAsync(http).ConfigureAwait(false);
                }
                catch (DiscordHttpApiException httpEx)
                {
                    log.LogError($"[ConnectLoop:GetGatewayUrl] {httpEx}");
                    log.LogError("[ConnectLoop] No gateway URL to connect with, trying again in 5s...");
                    await Task.Delay(5000, cancellationToken).ConfigureAwait(false);

                    continue;
                }
                catch (Exception ex)
                {
                    log.LogError(ex);
                    log.LogVerbose("IO Error occured while getting/storing gateway URL, setting state to Disconnected.");
                    state = GatewayState.Disconnected;

                    gatewayFailureData = new GatewayFailureData(
                        "Failed to retrieve/store the Gateway URL because of an IO error.",
                        ShardFailureReason.IOError, ex);
                    handshakeCompleteEvent.Set();

                    OnFailure?.Invoke(this, gatewayFailureData);

                    break;
                }

                log.LogVerbose($"[ConnectLoop] gatewayUrl = {gatewayUrl}");


                // Make sure we dont try and connect too often
                await connectionRateLimiter.Invoke(cancellationToken).ConfigureAwait(false);

                // Wait if necessary
                if (nextConnectionDelayMs > 0)
                {
                    log.LogVerbose($"[ConnectLoop] Waiting {nextConnectionDelayMs}ms before connecting socket...");

                    await Task.Delay(nextConnectionDelayMs, cancellationToken).ConfigureAwait(false);
                    nextConnectionDelayMs = 0;
                }

                try
                {
                    // Attempt to connect
                    await socket.ConnectAsync(new Uri($"{gatewayUrl}?v={GATEWAY_VERSION}&encoding=json"), cancellationToken)
                        .ConfigureAwait(false);
                    
                    // At this point the socket has successfully connected
                    log.LogVerbose("[ConnectLoop] Socket connected successfully.");
                    break;
                }
                catch (WebSocketException wsex)
                {
                    UnsubscribeSocketEvents();

                    // Failed to connect
                    log.LogError("[ConnectLoop] Failed to connect: " +
                        $"{wsex.WebSocketErrorCode} ({(int)wsex.WebSocketErrorCode}), {wsex.Message}");

                    try
                    {
                        // Invalidate the cached URL since we failed to connect the socket
                        await localStorage.InvalidateGatewayUrlAsync();
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex);
                        log.LogVerbose("IO Error occured while invalidating gateway URL, setting state to Disconnected.");
                        state = GatewayState.Disconnected;

                        gatewayFailureData = new GatewayFailureData(
                            "Failed to update the Gateway URL because of an IO error.",
                            ShardFailureReason.IOError, ex);
                        handshakeCompleteEvent.Set();

                        OnFailure?.Invoke(this, gatewayFailureData);

                        break;
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
            {
                if (resume)
                    log.LogInfo("[ConnectLoop:Reconnection] Successfully resumed.");
                else
                    log.LogInfo("[ConnectLoop:Reconnection] Successfully created new session.");

                OnReconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        void SubscribeSocketEvents()
        {
            socket.OnHello += Socket_OnHello;
            socket.OnReconnectionRequired += Socket_OnReconnectionRequired;
            socket.OnFatalDisconnection += Socket_OnFatalDisconnection;
            socket.OnDispatch += Socket_OnDispatch;
        }

        void UnsubscribeSocketEvents()
        {
            socket.OnHello -= Socket_OnHello;
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
                socket.SendResumePayload(botToken, sessionId, lastSequence);
            else
                // Identify
                socket.SendIdentifyPayload(botToken, lastShardStartConfig.GatewayLargeThreshold, shard.Id, totalShards);
        }

        private void Socket_OnFatalDisconnection(object sender, GatewayCloseCode e)
        {
            if (isDisposed)
                return;

            log.LogVerbose("Fatal disconnection occured, setting state to Disconnected.");
            state = GatewayState.Disconnected;

            (string message, ShardFailureReason reason) = GatewayCloseCodeToReason(e);
            gatewayFailureData = new GatewayFailureData(message, reason, null);
            handshakeCompleteEvent.Set();

            OnFailure?.Invoke(this, gatewayFailureData);
        }

        void Socket_OnReconnectionRequired(object sender, ReconnectionEventArgs e)
        {
            if (isDisposed)
                return;

            if (connectTask == null || connectTask.IsCompleted)
            {
                handshakeCompleteEvent.Reset();

                log.LogVerbose("[OnReconnectionRequired] Beginning automatic reconnection...");
                connectTaskCancellationSource = new CancellationTokenSource();

                nextConnectionDelayMs = e.ConnectionDelayMs;

                connectTask = ConnectLoop(!e.CreateNewSession, connectTaskCancellationSource.Token);
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
                catch (ShardCacheException cex)
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

        (string message, ShardFailureReason reason) GatewayCloseCodeToReason(GatewayCloseCode closeCode)
        {
            switch (closeCode)
            {
                case GatewayCloseCode.InvalidShard:
                    return ("The shard configuration was invalid.", ShardFailureReason.ShardInvalid);
                case GatewayCloseCode.AuthenticationFailed:
                    return ("The shard failed to authenticate.", ShardFailureReason.AuthenticationFailed);
                case GatewayCloseCode.ShardingRequired:
                    return ("Additional sharding is required.", ShardFailureReason.ShardingRequired);
                default:
                    return ($"An unknown error occured while starting the shard: {closeCode}({(int)closeCode})",
                        ShardFailureReason.Unknown);
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                state = GatewayState.Disconnected;

                connectTaskCancellationSource?.Dispose();
                handshakeCompleteCancellationSource?.Dispose();
                socket?.Dispose();
            }
        }
    }
}
