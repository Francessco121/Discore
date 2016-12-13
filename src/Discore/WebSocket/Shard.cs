using Discore.WebSocket.Net;
using System;

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
        DiscoreLogger log;

        internal Shard(DiscordWebSocketApplication app, int shardId)
        {
            Application = app;
            Id = shardId;

            log = new DiscoreLogger($"Shard#{shardId}");

            Cache = new DiscoreCache();

            gateway = new Gateway(app, this);
            gateway.OnFatalDisconnection += Gateway_OnFatalDisconnection;

            Voice = new ShardVoiceManager(this, gateway);
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
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this shard has already been started.</exception>
        public void Start()
        {
            if (!isRunning)
            {
                isRunning = true;

                CleanUp();

                // Keep trying to make the initial connection until successful, or the shard is stopped.
                while (isRunning)
                {
                    if (gateway.Connect())
                    {
                        log.LogVerbose("Successfully connected to gateway.");
                        OnConnected?.Invoke(this, new ShardEventArgs(this));
                        break;
                    }
                    else
                        log.LogInfo("Failed to connect to gateway, trying again...");
                }
            }
            else
                throw new InvalidOperationException($"Shard {Id} has already been started!");
        }

        /// <summary>
        /// Stop this shard.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this shard is not running.</exception>
        public void Stop()
        {
            if (isRunning)
            {
                isRunning = false;

                CleanUp();
                // We aren't concerned with the return status of the gateway disconnection,
                // as it should only "fail" if the gateway was already disconnected.
                gateway.Disconnect();
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
            gateway.Dispose();
        }
    }
}
