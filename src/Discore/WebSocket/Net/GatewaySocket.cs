using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    partial class GatewaySocket : DiscordClientWebSocket
    {
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
        /// Called when the socket is disconnected due to a rate limit.
        /// The OnReconnectionRequired event will be fired right after this.
        /// </summary>
        public event EventHandler OnRateLimited;
        /// <summary>
        /// Called when the socket receives the HELLO payload.
        /// </summary>
        public event EventHandler OnHello;

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

        bool isDisposed;

        DiscoreLogger log;

        public GatewaySocket(string loggingName, int sequence, 
            GatewayRateLimiter outboundPayloadRateLimiter, GatewayRateLimiter gameStatusUpdateRateLimiter)
            : base(loggingName)
        {
            this.sequence = sequence;
            this.outboundPayloadRateLimiter = outboundPayloadRateLimiter;
            this.gameStatusUpdateRateLimiter = gameStatusUpdateRateLimiter;

            log = new DiscoreLogger(loggingName);
            
            InitializePayloadHandlers();
        }

        public override async Task DisconnectAsync(WebSocketCloseStatus closeStatus, string statusDescription, 
            CancellationToken cancellationToken)
        {
            // Disconnect the socket
            await base.DisconnectAsync(closeStatus, statusDescription, cancellationToken)
                .ConfigureAwait(false);

            // Cancel the heartbeat loop if it hasn't ended already
            heartbeatCancellationSource?.Cancel();
        }

        protected override void OnPayloadReceived(DiscordApiData payload)
        {
            GatewayOPCode op = (GatewayOPCode)payload.GetInteger("op");
            DiscordApiData data = payload.Get("d");

            PayloadCallback callback;
            if (payloadHandlers.TryGetValue(op, out callback))
                callback(payload, data);
            else
                log.LogWarning($"Missing handler for payload: {op} ({(int)op})");
        }

        protected override void OnCloseReceived(WebSocketCloseStatus closeStatus, string closeDescription)
        {
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
                    log.LogWarning("Sent gateway payload before we identified!");
                    OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(true));
                    break;
                case GatewayCloseCode.RateLimited:
                    // Doesn't require a new session, but we need to wait a bit.
                    log.LogWarning("Gateway is being rate limited!");
                    OnRateLimited?.Invoke(this, EventArgs.Empty);
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

            log.LogVerbose("[HeartbeatLoop] Running.");

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
                            log.LogError($"[HeartbeatLoop] Unexpected error occured while sending a heartbeat: {dwex}");
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
                    log.LogInfo("Gateway connection timed out.");

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
