using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    partial class GatewaySocket : DiscordClientWebSocket
    {
        /// <summary>
        /// Called when a dispatch payload is received.
        /// </summary>
        public event EventHandler<DispatchEventArgs> OnDispatch;
        /// <summary>
        /// Called when the socket has disconnected and requires reconnection.
        /// The argument specifies whether a new session is required.
        /// </summary>
        public event EventHandler<bool> OnReconnectionRequired;
        /// <summary>
        /// Called when the socket has disconnected with a code specifying that
        /// we cannot saftely reconnect.
        /// </summary>
        public event EventHandler<GatewayDisconnectCode> OnFatalDisconnection;
        /// <summary>
        /// Called when the socket is disconnected due to a rate limit.
        /// The OnReconnectionRequired event will be fired right after this.
        /// </summary>
        public event EventHandler OnRateLimited;

        GatewayRateLimiter connectionRateLimiter;
        GatewayRateLimiter outboundPayloadRateLimiter;
        GatewayRateLimiter gameStatusUpdateRateLimiter;

        int heartbeatInterval;
        int heartbeatTimeoutAt;

        DiscoreLogger log;

        public GatewaySocket()
            : base("Gateway")
        {
            log = new DiscoreLogger("GatewaySocket");

            // Up-to-date rate limit parameters: https://discordapp.com/developers/docs/topics/gateway#rate-limiting
            connectionRateLimiter = new GatewayRateLimiter(5, 1); // 1 connection attempt per 5 seconds
            outboundPayloadRateLimiter = new GatewayRateLimiter(60, 120); // 120 outbound payloads every 60 seconds
            gameStatusUpdateRateLimiter = new GatewayRateLimiter(60, 5); // 5 status updates per minute

            InitializePayloadHandlers();
        }

        public override async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            await connectionRateLimiter.Invoke().ConfigureAwait(false);
            await base.ConnectAsync(uri, cancellationToken);
        }

        protected override void OnPayloadReceived(DiscordApiData payload)
        {
            GatewayOPCode op = (GatewayOPCode)payload.GetInteger("op");
            DiscordApiData data = payload.Get("d");

            PayloadCallback callback;
            if (payloadHandlers.TryGetValue(op, out callback))
                callback(payload, data);
            else
                log.LogWarning($"Missing handler for payload: {op}({(int)op})");
        }

        protected override void OnCloseReceived(WebSocketCloseStatus closeStatus, string closeDescription)
        {
            GatewayDisconnectCode code = (GatewayDisconnectCode)closeStatus;
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
                    OnReconnectionRequired?.Invoke(this, true);
                    break;
                case GatewayDisconnectCode.NotAuthenticated:
                    // This really should never happen, but will require a new session.
                    log.LogWarning("Sent gateway payload before we identified!");
                    OnReconnectionRequired?.Invoke(this, true);
                    break;
                case GatewayDisconnectCode.RateLimited:
                    // Doesn't require a new session, but we need to wait a bit.
                    log.LogWarning("Gateway is being rate limited!");
                    OnRateLimited?.Invoke(this, EventArgs.Empty);
                    OnReconnectionRequired?.Invoke(this, false);
                    break;
                default:
                    // Safe to just resume
                    OnReconnectionRequired?.Invoke(this, false);
                    break;
            }
        }

        protected override void OnClosedPrematurely()
        {
            // Attempt to resume
            OnReconnectionRequired?.Invoke(this, false);
        }
    }
}
