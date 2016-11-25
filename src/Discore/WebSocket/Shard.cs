using Discore.WebSocket.Audio;
using Discore.WebSocket.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
            Id = shardId;

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

        /// <summary>
        /// Requests guild members from the Discord API, this can be used to retrieve
        /// offline members in a guild that is considered "large". "Large" guilds
        /// will not automatically have the offline members available.
        /// <para>
        /// Members requested here will be returned via the callback and available
        /// in <see cref="DiscordGuild.Members"/>.
        /// </para>
        /// </summary>
        /// <param name="callback">Action to be invoked if the members are successfully retrieved.</param>
        /// <param name="guild">The guild to retrieve members from.</param>
        /// <param name="query">String that the username starts with, or an empty string to return all members.</param>
        /// <param name="limit">Maximum number of members to retrieve or 0 to request all members matched.</param>
        public void RequestGuildMembers(Action<DiscordApiCacheIdSet<DiscordGuildMember>> callback, DiscordGuild guild, 
            string query = "", int limit = 0)
        {
            // Create GUILD_MEMBERS_CHUNK event handler
            EventHandler<Snowflake[]> eventHandler = null;
            eventHandler = (sender, ids) => 
            {
                // Unhook event handler
                InternalGateway.OnGuildMembersChunk -= eventHandler;

                // Return ids
                DiscordApiCacheIdSet<DiscordGuildMember> set = new DiscordApiCacheIdSet<DiscordGuildMember>(guild.Members);
                for (int i = 0; i < ids.Length; i++)
                    set.Add(ids[i]);

                callback(set);
            };

            // Hook in event handler
            InternalGateway.OnGuildMembersChunk += eventHandler;

            // Send gateway request
            InternalGateway.SendRequestGuildMembersPayload(guild.Id, query, limit);
        }

        /// <summary>
        /// Updates the status of the bot user.
        /// </summary>
        /// <param name="game">Either null, or an object with one key "name", representing the name of the game being played.</param>
        /// <param name="idleSince">Unix time (in milliseconds) of when the client went idle, or null if the client is not idle.</param>
        public void UpdateStatus(string game = null, int? idleSince = null)
        {
            InternalGateway.SendStatusUpdate(game, idleSince);
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

            isRunning = false;
            CleanUp();
            OnFailure?.Invoke(this, new ShardFailureEventArgs(this, reason));
        }

        /// <summary>
        /// Attempts to start this shard.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this shard has already been started.</exception>
        /// <returns>Returns whether this shard connected successfully.</returns>
        public bool Start()
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
                throw new InvalidOperationException($"Shard {Id} has already been started!");
        }

        /// <summary>
        /// Attempts to stop this shard.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this shard is not running.</exception>
        /// <returns>Returns whether this shard disconnected successfully.</returns>
        public bool Stop()
        {
            if (isRunning)
            {
                isRunning = false;

                CleanUp();

                return InternalGateway.Disconnect();
            }
            else
                throw new InvalidOperationException($"Shard {Id} has already been stopped!");
        }

        void CleanUp()
        {
            Guilds.Clear();
            Channels.Clear();
            Roles.Clear();
            DirectMessageChannels.Clear();
            Users.Clear();
            VoiceConnectionsTable.Clear();
        }

        public void Dispose()
        {
            InternalGateway.Dispose();
        }
    }
}
