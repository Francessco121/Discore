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
        public bool IsRunning => isRunning;

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
        public IDiscordGateway Gateway => gateway;

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
            gateway.OnReadyEvent += Gateway_OnReadyEvent;

            Voice = new ShardVoiceManager(this, gateway);
        }

        private void Gateway_OnReadyEvent(object sender, EventArgs e)
        {
            Cache.Clear();
        }

        private void Gateway_OnReconnected(object sender, EventArgs e)
        {
            OnReconnected?.Invoke(this, new ShardEventArgs(this));
        }

        private void Gateway_OnFatalDisconnection(object sender, GatewayCloseCode e)
        {
            ShardFailureReason reason = ShardFailureReason.Unknown;
            if (e == GatewayCloseCode.InvalidShard)
                reason = ShardFailureReason.ShardInvalid;
            else if (e == GatewayCloseCode.AuthenticationFailed)
                reason = ShardFailureReason.AuthenticationFailed;
            else if (e == GatewayCloseCode.ShardingRequired)
                reason = ShardFailureReason.ShardingRequired;

            isRunning = false;
            CleanUp();
            OnFailure?.Invoke(this, new ShardFailureEventArgs(this, reason));
        }

        /// <summary>
        /// Starts this shard.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this shard has already been started.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if this shard has been disposed.</exception>
        [Obsolete("Please use the asynchronous counterpart StartAsync(CancellationToken) instead.")]
        public void Start()
        {
            StartAsync(CancellationToken.None).Wait();
        }

        /// <summary>
        /// Starts this shard. 
        /// The returned task only finishes once the gateway successfully connects (or is canceled), 
        /// and will continue to retry until then.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this shard has already been started.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if this shard has been disposed.</exception>
        /// <exception cref="OperationCanceledException"></exception>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(gateway), "Shard has been disposed.");

            if (!isRunning)
            {
                isRunning = true;

                CleanUp();

                await gateway.ConnectAsync(cancellationToken).ConfigureAwait(false);

                log.LogInfo("Successfully connected to the Gateway.");
                OnConnected?.Invoke(this, new ShardEventArgs(this));
            }
            else
                throw new InvalidOperationException($"Shard {Id} has already been started!");
        }

        /// <summary>
        /// Stop this shard.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this shard is not running.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if this shard has been disposed.</exception>
        [Obsolete("Please use the asynchronous counterpart StopAsync() instead.")]
        public void Stop()
        {
            StopAsync().Wait();
        }

        /// <summary>
        /// Stop this shard.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this shard is not running.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if this shard has been disposed.</exception>
        public async Task StopAsync()
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(gateway), "Shard has been disposed.");

            if (isRunning)
            {
                isRunning = false;

                CleanUp();
                await gateway.DisconnectAsync().ConfigureAwait(false);

                log.LogInfo("Successfully disconnected from the Gateway.");
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
