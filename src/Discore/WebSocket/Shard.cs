using Discore.WebSocket.Net;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket
{
    public class Shard : IDisposable
    {
        /// <summary>
        /// Gets the id of this shard.
        /// </summary>
        public int Id { get; }
        /// <summary>
        /// Gets the websocket application this shard was created from.
        /// </summary>
        public DiscordWebSocketApplication Application { get; }
        /// <summary>
        /// Gets whether this shard is currently running.
        /// </summary>
        public bool IsRunning { get { return isRunning; } }

        /// <summary>
        /// Called when this shard first connects to the Discord gateway.
        /// </summary>
        public event EventHandler<ShardEventArgs> OnConnected;
        /// <summary>
        /// Called when the internal connection of this shard reconnected to the Discord gateway.
        /// <para>
        /// This can be used to reset things such as the user status,
        /// which are reset when reconnecting.
        /// </para>
        /// </summary>
        public event EventHandler<ShardEventArgs> OnReconnected;
        /// <summary>
        /// Called when this shard fails and cannot reconnect due to the error.
        /// </summary>
        public event EventHandler<ShardFailureEventArgs> OnFailure;

        /// <summary>
        /// Gets the local memory cache of data from the Discord API.
        /// </summary>
        public DiscoreCache Cache { get; }

        /// <summary>
        /// Gets the user used to authenticate this shard connection.
        /// Or null if the gateway is not currently connected.
        /// </summary>
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the gateway manager for this shard.
        /// </summary>
        public IDiscordGateway Gateway { get { return gateway; } }

        /// <summary>
        /// Gets the voice manager for this shard.
        /// </summary>
        public ShardVoiceManager Voice { get; }

        Gateway gateway;
        bool isRunning;
        bool isDisposed;
        DiscoreLogger log;

        internal Shard(DiscordWebSocketApplication app, int shardId)
        {
            Application = app;
            Id = shardId;

            log = new DiscoreLogger($"Shard#{shardId}");

            Cache = new DiscoreCache();

            gateway = new Gateway(app, this);
            gateway.OnFatalDisconnection += Gateway_OnFatalDisconnection;
            gateway.OnReconnected += Gateway_OnReconnected;

            Voice = new ShardVoiceManager(this, gateway);
        }

        private void Gateway_OnReconnected(object sender, EventArgs e)
        {
            OnReconnected?.Invoke(this, new ShardEventArgs(this));
        }

        private void Gateway_OnFatalDisconnection(object sender, GatewayDisconnectCode e)
        {
            ShardFailureReason reason = ShardFailureReason.Unknown;
            if (e == GatewayDisconnectCode.InvalidShard)
                reason = ShardFailureReason.ShardInvalid;
            else if (e == GatewayDisconnectCode.AuthenticationFailed)
                reason = ShardFailureReason.AuthenticationFailed;

            isRunning = false;
            CleanUp();
            OnFailure?.Invoke(this, new ShardFailureEventArgs(this, reason));
        }

        /// <summary>
        /// Starts this shard. 
        /// The returned task only finishes once the gateway successfully connects (or is cancelled), 
        /// and will continue to retry until then.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this shard has already been started.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if this shard has been disposed.</exception>
        /// <exception cref="TaskCanceledException"></exception>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(gateway), "Shard has been disposed.");

            if (!isRunning)
            {
                isRunning = true;

                CleanUp();

                // Keep trying to make the initial connection until successful, or the shard is stopped.
                while (isRunning)
                {
                    try
                    {
                        // If already connected or another connection is in progress then stop.
                        if (gateway.SocketState == DiscoreWebSocketState.Open 
                            || gateway.SocketState == DiscoreWebSocketState.Connecting)
                            break;

                        // Try to connect
                        if (await gateway.ConnectAsync(cancellationToken))
                        {
                            log.LogVerbose("Successfully connected to gateway.");
                            OnConnected?.Invoke(this, new ShardEventArgs(this));
                            break;
                        }
                        else
                            log.LogInfo("Failed to connect to gateway, trying again...");
                    }
                    catch (Exception ex)
                    {
                        log.LogInfo($"Failed to connect to gateway, trying again... Exception: {ex}");
                    }
                }
            }
            else
                throw new InvalidOperationException($"Shard {Id} has already been started!");
        }

        /// <summary>
        /// Stop this shard.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this shard is not running.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if this shard has been disposed.</exception>
        /// <exception cref="TaskCanceledException"></exception>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(gateway), "Shard has been disposed.");

            if (isRunning)
            {
                isRunning = false;

                CleanUp();
                await gateway.DisconnectAsync(cancellationToken);
            }
            else
                throw new InvalidOperationException($"Shard {Id} has already been stopped!");
        }

        void CleanUp()
        {
            Cache.Clear();
            Voice.Clear();
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                gateway.Dispose();
            }
        }
    }
}
