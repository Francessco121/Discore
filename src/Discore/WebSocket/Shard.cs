using Discore.Voice;
using Discore.WebSocket.Internal;
using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket
{
    public class Shard : IDisposable
    {
        /// <summary>
        /// Gets the ID of this shard.
        /// </summary>
        public int Id { get; }
        /// <summary>
        /// Gets whether this shard is currently running.
        /// </summary>
        public bool IsRunning => isRunning;

        /// <summary>
        /// Called when this shard first connects to the Discord Gateway.
        /// </summary>
        public event EventHandler<ShardEventArgs>? OnConnected;
        /// <summary>
        /// Called when this shard disconnects from the Discord Gateway for any reason.
        /// <para/>
        /// If a shard failure is the cause, <see cref="OnFailure"/> will be fired right before
        /// this event.
        /// </summary>
        public event EventHandler<ShardEventArgs>? OnDisconnected;
        /// <summary>
        /// Called when the internal connection of this shard reconnected to the Discord Gateway.
        /// <para>
        /// This can be used to reset things such as the user status,
        /// which is cleared when a new session has been created.
        /// </para>
        /// </summary>
        public event EventHandler<ShardReconnectedEventArgs>? OnReconnected;
        /// <summary> 
        /// Called when this shard fails and cannot reconnect due to the error. 
        /// </summary> 
        public event EventHandler<ShardFailureEventArgs>? OnFailure;

        /// <summary>
        /// Gets the ID of the user used to authenticate this shard connection.
        /// Or null if the gateway is not currently connected.
        /// </summary>
        public Snowflake? UserId { get; internal set; }

        /// <summary>
        /// Gets the gateway manager for this shard.
        /// </summary>
        public IDiscordGateway Gateway => gateway;

        /// <summary>
        /// Gets the voice manager for this shard.
        /// </summary>
        public ShardVoiceManager Voice { get; }

        bool isRunning;
        bool isDisposed;

        readonly Gateway gateway;
        readonly DiscoreLogger log;

        readonly AsyncManualResetEvent stoppedResetEvent;

        /// <summary>
        /// Creates a new Discord WebSocket shard.
        /// </summary>
        /// <param name="botToken">The Discord bot token to authenticate with.</param>
        /// <param name="shardId">The ID of the shard or zero if this is the only shard.</param>
        /// <param name="totalShards">The total number of shards for the bot or one if there is only one shard.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="botToken"/> is null.</exception>
        public Shard(string botToken, int shardId, int totalShards)
        {
            if (botToken == null)
                throw new ArgumentNullException(nameof(botToken));

            Id = shardId;

            log = new DiscoreLogger($"Shard#{shardId}");

            stoppedResetEvent = new AsyncManualResetEvent(true);

            gateway = new Gateway(botToken, this, totalShards);
            gateway.OnFailure += Gateway_OnFailure;
            gateway.OnReconnected += Gateway_OnReconnected;

            Voice = new ShardVoiceManager(this);
        }

        private void Gateway_OnReconnected(object sender, GatewayReconnectedEventArgs e)
        {
            OnReconnected?.Invoke(this, new ShardReconnectedEventArgs(this, e.IsNewSession));
        }

        private void Gateway_OnFailure(object sender, GatewayFailureData e)
        {
            isRunning = false;
            CleanUp();

            stoppedResetEvent.Set();

            try
            {
                OnFailure?.Invoke(this, new ShardFailureEventArgs(this, e.Message, e.Reason, e.Exception));
            }
            finally
            {
                OnDisconnected?.Invoke(this, new ShardEventArgs(this));
            }
        }

        /// <summary>
        /// Starts this shard. 
        /// The returned task only finishes once the gateway successfully connects (or is canceled), 
        /// and will continue to retry until then.
        /// </summary>
        /// <param name="intents">
        /// A bitwise OR of Gateway event groups to subscribe to.
        /// <para/>
        /// Gateway events in groups that are not specified here will never be fired.
        /// For example, to receive 'MessageCreate' events for guild messages, specify <see cref="GatewayIntent.GuildMessages"/>.
        /// </param>
        /// <exception cref="InvalidOperationException">Thrown if this shard has already been started.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if this shard has been disposed.</exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="ShardStartException">Thrown if the shard fails to start.</exception>
        public Task StartAsync(GatewayIntent intents, CancellationToken? cancellationToken = null)
        {
            return StartAsync(new ShardStartConfig { Intents = intents }, cancellationToken);
        }

        /// <summary>
        /// Starts this shard. 
        /// The returned task only finishes once the gateway successfully connects (or is canceled), 
        /// and will continue to retry until then.
        /// </summary>
        /// <param name="config">A set of options to use when starting the shard.</param>
        /// <exception cref="ArgumentNullException">Thrown if config is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if this shard has already been started.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if this shard has been disposed.</exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="ShardStartException">Thrown if the shard fails to start.</exception>
        public async Task StartAsync(ShardStartConfig config, CancellationToken? cancellationToken = null)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(gateway), "Shard has been disposed.");
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!isRunning)
            {
                isRunning = true;

                CleanUp();

                CancellationToken ct = cancellationToken ?? CancellationToken.None;

                try
                {
                    await gateway.ConnectAsync(config, ct).ConfigureAwait(false);
                }
                catch (GatewayHandshakeException ex)
                {
                    isRunning = false;
                    CleanUp();

                    stoppedResetEvent.Set();

                    GatewayFailureData failureData = ex.FailureData;
                    throw new ShardStartException(failureData.Message, this, failureData.Reason, failureData.Exception);
                }
                catch
                {
                    isRunning = false;
                    CleanUp();

                    stoppedResetEvent.Set();

                    throw;
                }

                log.LogInfo("Successfully connected to the Gateway.");

                stoppedResetEvent.Reset();

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

                stoppedResetEvent.Set();

                OnDisconnected?.Invoke(this, new ShardEventArgs(this));
            }
            else
                throw new InvalidOperationException($"Shard {Id} has already been stopped!");
        }

        /// <summary>
        /// Returns a task that completes when this shard is stopped either normally or from an error.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the wait. Will not close the shard on cancellation.</param>
        /// <exception cref="OperationCanceledException">Thrown if the passed cancellation token is cancelled.</exception>
        public Task WaitUntilStoppedAsync(CancellationToken? cancellationToken = null)
        {
            return stoppedResetEvent.WaitAsync(cancellationToken ?? CancellationToken.None);
        }

        void CleanUp()
        {
            UserId = null;

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
