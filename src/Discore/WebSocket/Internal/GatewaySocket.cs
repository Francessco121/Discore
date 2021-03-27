using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket.Internal
{
    partial class GatewaySocket : DiscordClientWebSocket
    {
        public delegate Task HelloCallback();

        public int Sequence => sequence;

        /// <summary>
        /// Called when a dispatch payload is received.
        /// </summary>
        public event EventHandler<DispatchEventArgs> OnDispatch;
        /// <summary>
        /// Called when the socket has disconnected and requires reconnection.
        /// The argument specifies whether a new session is required.
        /// </summary>
        public event EventHandler<ReconnectionEventArgs> OnReconnectionRequired;
        /// <summary>
        /// Called when the socket has disconnected with a code specifying that
        /// we cannot saftely reconnect.
        /// </summary>
        public event EventHandler<GatewayCloseCode> OnFatalDisconnection;

        /// <summary>
        /// Called when the socket receives the HELLO payload.
        /// </summary>
        public HelloCallback OnHello { get; set; }

        GatewayRateLimiter identifyRateLimiter;
        GatewayRateLimiter outboundPayloadRateLimiter;
        GatewayRateLimiter gameStatusUpdateRateLimiter;

        int sequence;

        Task heartbeatTask;
        CancellationTokenSource heartbeatCancellationSource;
        int heartbeatInterval;
        bool receivedHeartbeatAck;

        /// <summary>
        /// The gateway is known to send more than one HELLO payload occasionally,
        /// this is used to ensure we don't respond to it more than once on accident.
        /// </summary>
        bool receivedHello;

        /// <summary>
        /// Whether disconnection was initiated on our end.
        /// </summary>
        bool areWeDisconnecting;
        
        bool isDisposed;

        DiscoreLogger log;

        public GatewaySocket(string loggingName, int sequence, 
            GatewayRateLimiter outboundPayloadRateLimiter, GatewayRateLimiter gameStatusUpdateRateLimiter,
            GatewayRateLimiter identifyRateLimiter)
            : base(loggingName)
        {
            this.sequence = sequence;
            this.outboundPayloadRateLimiter = outboundPayloadRateLimiter;
            this.gameStatusUpdateRateLimiter = gameStatusUpdateRateLimiter;
            this.identifyRateLimiter = identifyRateLimiter;

            log = new DiscoreLogger(loggingName);
            
            InitializePayloadHandlers();
        }

        public override async Task DisconnectAsync(WebSocketCloseStatus closeStatus, string statusDescription, 
            CancellationToken cancellationToken)
        {
            areWeDisconnecting = true;

            // Disconnect the socket
            await base.DisconnectAsync(closeStatus, statusDescription, cancellationToken)
                .ConfigureAwait(false);

            // Cancel the heartbeat loop if it hasn't ended already
            heartbeatCancellationSource?.Cancel();
        }

        protected override async Task OnPayloadReceived(DiscordApiData payload)
        {
            GatewayOPCode op = (GatewayOPCode)payload.GetInteger("op");
            DiscordApiData data = payload.Get("d");

            PayloadCallback callback;
            if (payloadHandlers.TryGetValue(op, out callback))
            {
                if (callback.Synchronous != null)
                    callback.Synchronous(payload, data);
                else
                    await callback.Asynchronous(payload, data).ConfigureAwait(false);
            }
            else
                log.LogWarning($"Missing handler for payload: {op} ({(int)op})");
        }

        protected override void OnCloseReceived(WebSocketCloseStatus closeStatus, string closeDescription)
        {
            // If we initiated a disconnect, this is just the remote end's acknowledgment
            // and we should not start reconnecting
            if (areWeDisconnecting) return;

            GatewayCloseCode code = (GatewayCloseCode)closeStatus;
            switch (code)
            {
                case GatewayCloseCode.InvalidShard:
                case GatewayCloseCode.AuthenticationFailed:
                case GatewayCloseCode.ShardingRequired:
                    // Not safe to reconnect
                    log.LogError($"[{code} ({(int)code})] Unsafe to continue, NOT reconnecting gateway.");
                    OnFatalDisconnection?.Invoke(this, code);
                    break;
                case GatewayCloseCode.InvalidSeq:
                case GatewayCloseCode.InvalidSession:
                case GatewayCloseCode.SessionTimeout:
                case GatewayCloseCode.UnknownError:
                    // Safe to reconnect, but needs a new session.
                    OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(true));
                    break;
                case GatewayCloseCode.NotAuthenticated:
                    // This really should never happen, but will require a new session.
                    log.LogWarning("[NotAuthenticated] Sent gateway payload before we identified!");
                    OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(true));
                    break;
                case GatewayCloseCode.RateLimited:
                    // Doesn't require a new session, but we need to wait a bit.
                    log.LogError("Gateway is being rate limited!"); // Error level because we have code that should prevent this.
                    OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(false, 5000));
                    break;
                default:
                    // Safe to just resume
                    OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(false));
                    break;
            }
        }

        protected override void OnClosedPrematurely()
        {
            // Attempt to resume
            OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(false));
        }

        async Task HeartbeatLoop()
        {
            // Default to true for the first heartbeat payload we send.
            receivedHeartbeatAck = true;

            log.LogVerbose("[HeartbeatLoop] Begin.");

            while (State == WebSocketState.Open && !heartbeatCancellationSource.IsCancellationRequested)
            {
                if (receivedHeartbeatAck)
                {
                    receivedHeartbeatAck = false;

                    try
                    {
                        // Send heartbeat
                        await SendHeartbeatPayload().ConfigureAwait(false);
                    }
                    catch (InvalidOperationException)
                    {
                        // Socket was closed between the loop check and sending the heartbeat
                        break;
                    }
                    catch (DiscordWebSocketException dwex)
                    {
                        // Expected to be the socket closing while sending a heartbeat
                        if (dwex.Error != DiscordWebSocketError.ConnectionClosed)
                            // Unexpected errors may not be the socket closing/aborting, so just log and loop around.
                            log.LogError("[HeartbeatLoop] Unexpected error occured while sending a heartbeat: " +
                                $"code = {dwex.Error}, error = {dwex}");
                        else
                            break;
                    }

                    try
                    {
                        // Wait heartbeat interval
                        await Task.Delay(heartbeatInterval, heartbeatCancellationSource.Token)
                            .ConfigureAwait(false);
                    }
                    catch (ObjectDisposedException)
                    {
                        // GatewaySocket was disposed between sending a heartbeat payload and beginning to wait
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        // Socket is disconnecting
                        break;
                    }
                }
                else
                {
                    // Gateway connection has timed out
                    log.LogInfo("Gateway connection timed out (did not receive ack for last heartbeat).");

                    // Notify that this connection needs to be resumed
                    OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(false));

                    break;
                }
            }

            log.LogVerbose($"[HeartbeatLoop] Done. isDisposed = {isDisposed}");
        }

        public override void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                heartbeatCancellationSource?.Dispose();
                base.Dispose();
            }
        }
    }
}
