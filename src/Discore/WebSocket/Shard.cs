using Discore.WebSocket.Audio;
using Discore.WebSocket.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Discore.WebSocket
{
    public class Shard : IDisposable
    {
        public int ShardId { get; }
        public DiscordWebSocketApplication Application { get; }
        public bool IsActive { get { return isRunning; } }

        /// <summary>
        /// Called when this shard first connects to the Discord gateway.
        /// </summary>
        public event EventHandler<ShardEventArgs> OnConnected;
        /// <summary>
        /// Called when this shard fails and cannot reconnect due to the error.
        /// </summary>
        public event EventHandler<ShardFailureEventArgs> OnFailure;

        /// <summary>
        /// Gets a table of all guilds managed by this shard.
        /// </summary>
        public DiscordApiCacheTable<DiscordGuild> Guilds { get; }
        /// <summary>
        /// Gets a table of all channels managed by this shard.
        /// </summary>
        public DiscordApiCacheTable<DiscordChannel> Channels { get; }
        /// <summary>
        /// Gets a table of all roles managed by this shard.
        /// </summary>
        public DiscordApiCacheTable<DiscordRole> Roles { get; }
        /// <summary>
        /// Gets a table of all DM channels managed by this shard.
        /// </summary>
        public DiscordApiCacheTable<DiscordDMChannel> DirectMessageChannels { get; }
        /// <summary>
        /// Gets a table of all users managed by this shard.
        /// </summary>
        public DiscordApiCacheTable<DiscordUser> Users { get; }

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
            ShardId = shardId;

            log = new DiscoreLogger($"Shard#{shardId}");

            Guilds = new DiscordApiCacheTable<DiscordGuild>();
            Channels = new DiscordApiCacheTable<DiscordChannel>();
            Roles = new DiscordApiCacheTable<DiscordRole>();
            DirectMessageChannels = new DiscordApiCacheTable<DiscordDMChannel>();
            Users = new DiscordApiCacheTable<DiscordUser>();
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
            if (VoiceConnectionsTable.TryRemove(voiceChannel.Guild.Id, out connection))
                // Close any existing connection.
                connection.Disconnect();

            DiscordGuildMember member;
            if (voiceChannel.Guild.Members.TryGetValue(User.Id, out member))
            {
                connection = new DiscordVoiceConnection(this, voiceChannel.Guild, member, voiceChannel);
                if (VoiceConnectionsTable.TryAdd(voiceChannel.Guild.Id, connection))
                {
                    // Initiate connection
                    InternalGateway.SendVoiceStateUpdatePayload(voiceChannel.Guild.Id, voiceChannel.Id, false, false);
                    return connection;
                }
                else
                    // Connection already exists, just return the existing one.
                    return VoiceConnectionsTable[voiceChannel.Guild.Id];
            }
            else
                // This really should never ever ever happen.
                throw new ArgumentException("The current authenticated user is not a member of the specified guild.", 
                    nameof(voiceChannel));
        }

        private void Gateway_OnFatalDisconnection(object sender, GatewayDisconnectCode e)
        {
            ShardFailureReason reason = ShardFailureReason.Unknown;
            if (e == GatewayDisconnectCode.InvalidShard)
                reason = ShardFailureReason.ShardInvalid;
            else if (e == GatewayDisconnectCode.AuthenticationFailed)
                reason = ShardFailureReason.AuthenticationFailed;

            OnFailure?.Invoke(this, new ShardFailureEventArgs(this, reason));
        }

        internal bool Start()
        {
            if (!isRunning)
            {
                isRunning = true;

                if (InternalGateway.Connect())
                {
                    log.LogInfo("Successfully connected to gateway");
                    OnConnected?.Invoke(this, new ShardEventArgs(this));
                    return true;
                }
                else
                    return false;
            }
            else
                throw new InvalidOperationException($"Shard {ShardId} has already been started!");
        }

        internal bool Stop()
        {
            if (isRunning)
            {
                isRunning = false;

                Guilds.Clear();
                Channels.Clear();
                Roles.Clear();
                DirectMessageChannels.Clear();
                Users.Clear();
                VoiceConnectionsTable.Clear();

                return InternalGateway.Disconnect();
            }
            else
                throw new InvalidOperationException($"Shard {ShardId} has already been stopped!");
        }

        public void Dispose()
        {
            InternalGateway.Dispose();
        }
    }
}
