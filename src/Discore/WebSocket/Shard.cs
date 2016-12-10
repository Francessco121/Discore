using Discore.Voice;
using Discore.WebSocket.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
        public IDiscordGateway Gateway { get { return InternalGateway; } }

        /// <summary>
        /// Gets a list of all voice connections for the current authenticated user.
        /// </summary>
        public IReadOnlyCollection<DiscordVoiceConnection> VoiceConnections
        {
            get
            {
                // Now if only a ReadOnlyCollection could take an ICollection...
                DiscordVoiceConnection[] connections = new DiscordVoiceConnection[VoiceConnections.Count];
                VoiceConnectionsTable.Values.CopyTo(connections, 0);

                return connections;
            }
        }

        internal ConcurrentDictionary<Snowflake, DiscordVoiceConnection> VoiceConnectionsTable;
        internal Gateway InternalGateway;

        bool isRunning;
        DiscoreLogger log;

        internal Shard(DiscordWebSocketApplication app, int shardId)
        {
            Application = app;
            Id = shardId;

            log = new DiscoreLogger($"Shard#{shardId}");

            Cache = new DiscoreCache();

            VoiceConnectionsTable = new ConcurrentDictionary<Snowflake, DiscordVoiceConnection>();

            InternalGateway = new Gateway(app, this);
            InternalGateway.OnFatalDisconnection += Gateway_OnFatalDisconnection;
        }

        /// <summary>
        /// Attempts to retrieve a voice connection for the current user,
        /// by the guild the connection is in.
        /// </summary>
        public bool TryGetVoiceConnection(DiscordGuild guild, out DiscordVoiceConnection connection)
        {
            return VoiceConnectionsTable.TryGetValue(guild.Id, out connection);
        }

        internal DiscordVoiceConnection ConnectToVoice(DiscordGuildVoiceChannel voiceChannel)
        {
            DiscordVoiceConnection connection;
            if (VoiceConnectionsTable.TryRemove(voiceChannel.GuildId, out connection))
                // Close any existing connection.
                connection.Disconnect();

            // Get the guild cache
            DiscoreGuildCache guildCache;
            if (Cache.Guilds.TryGetValue(voiceChannel.GuildId, out guildCache))
            {
                // Get the authenticated user's guild member from cache
                DiscoreMemberCache memberCache;
                if (guildCache.Members.TryGetValue(User.Id, out memberCache))
                {
                    // Create the new connection
                    connection = new DiscordVoiceConnection(this, guildCache, memberCache, voiceChannel);
                    if (VoiceConnectionsTable.TryAdd(voiceChannel.GuildId, connection))
                    {
                        // Initiate connection
                        InternalGateway.SendVoiceStateUpdatePayload(voiceChannel.GuildId, voiceChannel.Id, false, false);
                        return connection;
                    }
                    else
                        // Connection already exists, just return the existing one.
                        return VoiceConnectionsTable[voiceChannel.GuildId];
                }
                else
                    // This really should never ever ever happen.
                    throw new ArgumentException("The current authenticated user is not a member of the specified guild.",
                        nameof(voiceChannel));
            }
            else
                throw new DiscoreCacheException("The specified guild does not exist in the local cache.");
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
                    if (InternalGateway.Connect())
                    {
                        log.LogVerbose("Successfully connected to gateway.");
                        OnConnected?.Invoke(this, new ShardEventArgs(this));
                        break;
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
        public void Stop()
        {
            if (isRunning)
            {
                isRunning = false;

                CleanUp();
                // We aren't concerned with the return status of the gateway disconnection,
                // as it should only "fail" if the gateway was already disconnected.
                InternalGateway.Disconnect();
            }
            else
                throw new InvalidOperationException($"Shard {Id} has already been stopped!");
        }

        void CleanUp()
        {
            Cache.Clear();
            VoiceConnectionsTable.Clear();
        }

        public void Dispose()
        {
            InternalGateway.Dispose();
        }
    }
}
