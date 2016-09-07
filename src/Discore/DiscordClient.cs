using Discore.Audio;
using Discore.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore
{
    /// <summary>
    /// The main interface with the Discord API.
    /// </summary>
    public sealed class DiscordClient : IDiscordClient, IDisposable
    {
        #region Events
        /// <summary>
        /// Called when a fatal error occurs and the client is no longer connected.
        /// </summary>
        public event EventHandler<DiscordClientExceptionEventArgs> OnFatalError;
        /// <summary>
        /// Called when this client connects to the Discord API.
        /// </summary>
        public event EventHandler OnConnected;
        /// <summary>
        /// Called when a voice connection is established.
        /// </summary>
        public event EventHandler<VoiceClientEventArgs> OnVoiceClientConnected;
        /// <summary>
        /// Called when a voice connection is terminated.
        /// </summary>
        public event EventHandler<VoiceClientEventArgs> OnVoiceClientDisconnected;

        /// <summary>
        /// Called when a <see cref="DiscordChannel"/> is created.
        /// </summary>
        public event EventHandler<ChannelEventArgs> OnChannelCreated;
        /// <summary>
        /// Called when a <see cref="DiscordGuildChannel"/> is updated.
        /// </summary>
        public event EventHandler<ChannelEventArgs> OnChannelUpdated;
        /// <summary>
        /// Called when a <see cref="DiscordChannel"/> is deleted/closed.
        /// </summary>
        public event EventHandler<ChannelEventArgs> OnChannelDeleted;
        /// <summary>
        /// Called when a <see cref="DiscordGuild"/> is created.
        /// </summary>
        public event EventHandler<GuildEventArgs> OnGuildCreated;
        /// <summary>
        /// Called when a <see cref="DiscordGuild"/> is updated.
        /// </summary>
        public event EventHandler<GuildEventArgs> OnGuildUpdated;
        /// <summary>
        /// Called when a <see cref="DiscordGuild"/> is deleted.
        /// </summary>
        public event EventHandler<GuildEventArgs> OnGuildDeleted;
        /// <summary>
        /// Called when a <see cref="DiscordUser"/> is banned from a <see cref="DiscordGuild"/>.
        /// </summary>
        public event EventHandler<GuildUserEventArgs> OnGuildBanAdd;
        /// <summary>
        /// Called when a <see cref="DiscordUser"/> is unbanned from a <see cref="DiscordGuild"/>.
        /// </summary>
        public event EventHandler<GuildUserEventArgs> OnGuildBanRemove;
        /// <summary>
        /// Called the the <see cref="DiscordEmoji"/>s of a <see cref="DiscordGuild"/> are updated.
        /// </summary>
        public event EventHandler<GuildEventArgs> OnGuildEmojisUpdated;
        /// <summary>
        /// Called when an <see cref="DiscordIntegration"/> of a <see cref="DiscordGuild"/> is updated.
        /// </summary>
        public event EventHandler<IntegrationEventArgs> OnGuildIntegrationsUpdated;
        /// <summary>
        /// Called when a <see cref="DiscordUser"/> joins a <see cref="DiscordGuild"/>.
        /// </summary>
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberAdded;
        /// <summary>
        /// Called when a <see cref="DiscordGuildMember"/> is updated. 
        /// </summary>
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberUpdated;
        /// <summary>
        /// Called when a <see cref="DiscordUser"/> leaves or is removed from a <see cref="DiscordGuild"/>.
        /// </summary>
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberRemoved;
        /// <summary>
        /// Called when a <see cref="DiscordRole"/> is created.
        /// </summary>
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleCreated;
        /// <summary>
        /// Called when a <see cref="DiscordRole"/> is updated.
        /// </summary>
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleUpdated;
        /// <summary>
        /// Called when a <see cref="DiscordRole"/> is deleted.
        /// </summary>
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleDeleted;
        /// <summary>
        /// Called when a <see cref="DiscordUser"/> sends a <see cref="DiscordMessage"/>.
        /// </summary>
        public event EventHandler<MessageEventArgs> OnMessageCreated;
        /// <summary>
        /// Called when a <see cref="DiscordUser"/> edits a <see cref="DiscordMessage"/>.
        /// </summary>
        public event EventHandler<MessageEventArgs> OnMessageUpdated;
        /// <summary>
        /// Called when a <see cref="DiscordMessage"/> is deleted.
        /// </summary>
        public event EventHandler<MessageEventArgs> OnMessageDeleted;
        /// <summary>
        /// Called when a <see cref="DiscordGuildMember"/> starts typing.
        /// </summary>
        public event EventHandler<TypingStartEventArgs> OnTypingStarted;
        #endregion

        /// <summary>
        /// Gets all <see cref="DiscordGuild"/>s known to this client.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, DiscordGuild>> Guilds
        {
            get { return cache.GetList<DiscordGuild>(); }
        }
        /// <summary>
        /// Gets all <see cref="DiscordUser"/>s known to this client.
        /// </summary>
        public IReadOnlyList<KeyValuePair<string, DiscordUser>> Users
        {
            get { return cache.GetList<DiscordUser>(); }
        }

        /// <summary>
        /// Gets the <see cref="DiscordUser"/> authenticated with this <see cref="DiscordClient"/>.
        /// </summary>
        public DiscordUser User { get; private set; }
        /// <summary>
        /// Gets whether or not this <see cref="DiscordClient"/> is connected to the Discord API.
        /// </summary>
        public bool IsConnected { get { return running && GatewaySocket.IsConnected; } }

        /// <summary>
        /// Gets the data cache of this <see cref="DiscordClient"/>.
        /// </summary>
        public DiscordApiCache Cache { get { return cache; } }
        /// <summary>
        /// Gets the Discord gateway interface used by this <see cref="DiscordClient"/>.
        /// </summary>
        public IDiscordGateway Gateway { get { return GatewaySocket; } }
        /// <summary>
        /// Gets the Discord REST interface used by this <see cref="DiscordClient"/>.
        /// </summary>
        public IDiscordRestClient Rest { get { return RestClient; } }

        internal ConcurrentDictionary<DiscordGuild, DiscordVoiceClient> VoiceClients { get; }
        internal GatewaySocket GatewaySocket { get; }
        internal RestClient RestClient { get; }
        internal DiscordApiCacheHelper CacheHelper { get; }
        internal DiscordApiInfoCache ApiInfo { get; }

        ConcurrentDictionary<string, Action<DiscordApiData>> gatewayEventHandlers;

        bool running;

        DiscordLogger log;
        DiscordApiCache cache;

        /// <summary>
        /// Creates a new <see cref="DiscordClient"/> instance.
        /// </summary>
        public DiscordClient()
        {
            log = new DiscordLogger("DiscordClient");
            cache = new DiscordApiCache();
            CacheHelper = new DiscordApiCacheHelper(this, cache);
            ApiInfo = new DiscordApiInfoCache();

            GatewaySocket = new GatewaySocket(this);
            RestClient = new RestClient(this);
           
            VoiceClients = new ConcurrentDictionary<DiscordGuild, DiscordVoiceClient>();

            gatewayEventHandlers = new ConcurrentDictionary<string, Action<DiscordApiData>>();
            gatewayEventHandlers.TryAdd("CHANNEL_CREATE", HandleChannelCreate);
            gatewayEventHandlers.TryAdd("CHANNEL_UPDATE", HandleChannelUpdate);
            gatewayEventHandlers.TryAdd("CHANNEL_DELETE", HandleChannelDelete);

            gatewayEventHandlers.TryAdd("GUILD_CREATE", HandleGuildCreate);
            gatewayEventHandlers.TryAdd("GUILD_UPDATE", HandleGuildUpdate);
            gatewayEventHandlers.TryAdd("GUILD_DELETE", HandleGuildDelete);
            gatewayEventHandlers.TryAdd("GUILD_BAN_ADD", HandleGuildBanAdd);
            gatewayEventHandlers.TryAdd("GUILD_BAN_REMOVE", HandleGuildBanRemove);
            gatewayEventHandlers.TryAdd("GUILD_EMOJI_UPDATE", HandleGuildEmojiUpdate);
            gatewayEventHandlers.TryAdd("GUILD_INTEGRATIONS_UPDATE", HandleGuildIntegrationsUpdate);

            gatewayEventHandlers.TryAdd("GUILD_MEMBER_ADD", HandleGuildMemberAdd);
            gatewayEventHandlers.TryAdd("GUILD_MEMBER_UPDATE", HandleGuildMemberUpdate);
            gatewayEventHandlers.TryAdd("GUILD_MEMBER_REMOVE", HandleGuildMemberRemove);
            gatewayEventHandlers.TryAdd("GUILD_MEMBERS_CHUNK", HandleGuildMembersChunk);

            gatewayEventHandlers.TryAdd("GUILD_ROLE_CREATE", HandleGuildRoleCreate);
            gatewayEventHandlers.TryAdd("GUILD_ROLE_UPDATE", HandleGuildRoleUpdate);
            gatewayEventHandlers.TryAdd("GUILD_ROLE_DELETE", HandleGuildRoleDelete);

            gatewayEventHandlers.TryAdd("MESSAGE_CREATE", HandleMessageCreate);
            gatewayEventHandlers.TryAdd("MESSAGE_UPDATE", HandleMessageUpdate);
            gatewayEventHandlers.TryAdd("MESSAGE_DELETE", HandleMessageDelete);
            gatewayEventHandlers.TryAdd("MESSAGE_DELETE_BULK", HandleMessageDeleteBulk);

            gatewayEventHandlers.TryAdd("PRESENCE_UPDATE", HandlePresenceUpdate);
            gatewayEventHandlers.TryAdd("TYPING_START", HandleTypingStarted);

            gatewayEventHandlers.TryAdd("USER_UPDATE", HandleUserUpdate);

            gatewayEventHandlers.TryAdd("VOICE_STATE_UPDATE", HandleVoiceStateUpdate);

            GatewaySocket.OnReadyEvent += Gateway_OnReadyEvent;
            GatewaySocket.OnUnhandledEvent += Gateway_OnUnhandledEvent;
            GatewaySocket.OnVoiceClientConnected += Gateway_OnVoiceClientConnected;
            GatewaySocket.OnFatalError += GatewaySocket_OnFatalError;
        }

        private void GatewaySocket_OnFatalError(object sender, Exception ex)
        {
            Disconnect().Wait();
            OnFatalError?.Invoke(this, new DiscordClientExceptionEventArgs(this, ex));
        }

        private void Gateway_OnVoiceClientConnected(object sender, VoiceClientEventArgs e)
        {
            e.VoiceClient.OnDisconnected += VoiceClient_OnDisconnected;
            e.VoiceClient.OnFatalError += VoiceClient_OnFatalError;
            OnVoiceClientConnected?.Invoke(this, e);
        }

        private void VoiceClient_OnFatalError(object sender, VoiceClientExceptionEventArgs e)
        {
            e.VoiceClient.OnDisconnected -= VoiceClient_OnDisconnected;
            e.VoiceClient.OnFatalError -= VoiceClient_OnFatalError;
            OnVoiceClientDisconnected?.Invoke(this, new VoiceClientEventArgs(e.VoiceClient));
        }

        private void VoiceClient_OnDisconnected(object sender, VoiceClientEventArgs e)
        {
            e.VoiceClient.OnDisconnected -= VoiceClient_OnDisconnected;
            e.VoiceClient.OnFatalError -= VoiceClient_OnFatalError;
            OnVoiceClientDisconnected?.Invoke(this, e);
        }

        private void Gateway_OnReadyEvent(object sender, DiscordApiData data)
        {
            DiscordApiData userData = data.Get("user");
            User = cache.AddOrUpdate(userData.GetString("id"), userData, () => { return new DiscordUser(this); });

            foreach (DiscordApiData dmChannelData in data.GetArray("private_channels"))
            {
                string dmChannelId = dmChannelData.GetString("id");

                DiscordDMChannel dmChannel = cache.AddOrUpdate(dmChannelId, dmChannelData, 
                    () => { return new DiscordDMChannel(this); });
                cache.SetAlias<DiscordChannel>(dmChannel);
            }

            // TODO: do something with data.guilds (Unavailable Guilds)

            log.LogInfo($"[READY] Using account '{User.Username}'");
            OnConnected?.Invoke(this, EventArgs.Empty);
        }

        private void Gateway_OnUnhandledEvent(string eventName, DiscordApiData data)
        {
            Action<DiscordApiData> handler;
            if (gatewayEventHandlers.TryGetValue(eventName, out handler))
                handler(data);
            else
                log.LogWarning($"[Unhandled Gateway Event] {eventName}");
        }

        /// <summary>
        /// Attempts to connect to the Discord API with the token 
        /// of the user/bot to authenticate as.
        /// </summary>
        /// <param name="token">The token of the user/bot to authenticate as.</param>
        /// <returns>Returns whether or not the client successfully connected.</returns>
        public async Task<bool> Connect(string token)
        {
            if (!running)
            {
                if (await GatewaySocket.Connect(token))
                {
                    RestClient.SetToken(token);

                    running = true;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>
        /// Disconnects this client from the Discord API.
        /// </summary>
        /// <returns>Returns an awaitable <see cref="Task"/>.</returns>
        public async Task Disconnect()
        {
            if (running)
            {
                running = false;

                foreach (DiscordVoiceClient client in VoiceClients.Values)
                    client.Disconnect();

                await GatewaySocket.Disconnect();
            }
        }

        /// <summary>
        /// Updates the status of the currently authenticated user.
        /// </summary>
        /// <param name="game">The game currently being played.</param>
        /// <param name="gameType">The type of game currently being played.</param>
        /// <param name="idleSince">The time in seconds the user has been idle.</param>
        public void UpdateStatus(string game, DiscordGameType gameType = DiscordGameType.Default, int? idleSince = null)
        {
            if (running)
                GatewaySocket.SendStatusUpdate(game, gameType, idleSince);
        }

        /// <summary>
        /// Attempts to get a <see cref="DiscordUser"/> by their id.
        /// </summary>
        /// <param name="id">The id of the <see cref="DiscordUser"/>.</param>
        /// <param name="user">The found <see cref="DiscordUser"/>.</param>
        /// <returns>Returns whether or not the <see cref="DiscordUser"/> was found.</returns>
        public bool TryGetUser(string id, out DiscordUser user)
        {
            return cache.TryGet(id, out user);
        }

        /// <summary>
        /// Attempts to get a <see cref="DiscordGuild"/> by its id.
        /// </summary>
        /// <param name="id">The id of the <see cref="DiscordGuild"/>.</param>
        /// <param name="guild">The found <see cref="DiscordGuild"/>.</param>
        /// <returns>Returns whether or not the <see cref="DiscordGuild"/> was found.</returns>
        public bool TryGetGuild(string id, out DiscordGuild guild)
        {
            return cache.TryGet(id, out guild);
        }

        /// <summary>
        /// Attempts to get a <see cref="DiscordDMChannel"/> by its id.
        /// </summary>
        /// <param name="id">The id of the <see cref="DiscordDMChannel"/>.</param>
        /// <param name="dmChannel">The found <see cref="DiscordDMChannel"/>.</param>
        /// <returns>Returns whether or not the <see cref="DiscordDMChannel"/> was found.</returns>
        public bool TryGetDirectMessageChannel(string id, out DiscordDMChannel dmChannel)
        {
            return cache.TryGet(id, out dmChannel);
        }

        /// <summary>
        /// Attempts to get a <see cref="DiscordChannel"/> by its id.
        /// </summary>
        /// <param name="id">The id of the <see cref="DiscordChannel"/>.</param>
        /// <param name="channel">The found <see cref="DiscordChannel"/>.</param>
        /// <returns>Returns whether or not the <see cref="DiscordChannel"/> was found.</returns>
        public bool TryGetChannel(string id, out DiscordChannel channel)
        {
            return cache.TryGet(id, out channel);
        }

        /// <summary>
        /// Attempts to get a <see cref="DiscordVoiceClient"/> by the <see cref="DiscordGuild"/> it is in.
        /// </summary>
        /// <param name="guild">The <see cref="DiscordGuild"/> the <see cref="DiscordVoiceClient"/> is in.</param>
        /// <param name="voiceClient">The found <see cref="DiscordVoiceClient"/>.</param>
        /// <returns>Returns where or not the <see cref="DiscordVoiceClient"/> was found.</returns>
        public bool TryGetVoiceClient(DiscordGuild guild, out DiscordVoiceClient voiceClient)
        {
            return VoiceClients.TryGetValue(guild, out voiceClient);
        }

        #region Gateway Event Handlers
        #region Channel Events
        void HandleChannelCreate(DiscordApiData data)
        {
            try
            {
                DiscordChannel channel = CacheHelper.CreateChannel(data);

                if (channel.ChannelType == DiscordChannelType.DirectMessage)
                {
                    DiscordDMChannel dmChannel = (DiscordDMChannel)channel;

                    log.LogVerbose($"[CHANNEL_CREATE] Received dm channel to '{dmChannel.Recipient.Username}'");
                    OnChannelCreated?.Invoke(this, new ChannelEventArgs(dmChannel));
                }
                else
                {
                    DiscordGuildChannel guildChannel = (DiscordGuildChannel)channel;

                    log.LogVerbose($"[CHANNEL_CREATE] Received channel '{guildChannel.Name}' for guild '{guildChannel.Guild.Name}'");
                    OnChannelCreated?.Invoke(this, new ChannelEventArgs(channel));
                }
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[CHANNEL_CREATE] {ex.Message}");
            }
        }

        void HandleChannelUpdate(DiscordApiData data)
        {
            try
            {
                DiscordGuildChannel channel = CacheHelper.UpdateChannel(data);

                log.LogVerbose($"[CHANNEL_UPDATE] Received channel '{channel.Name}' for guild '{channel.Guild.Name}'");
                OnChannelUpdated?.Invoke(this, new ChannelEventArgs(channel));
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[CHANNEL_UPDATE] {ex.Message}");
            }
        }

        void HandleChannelDelete(DiscordApiData data)
        {
            try
            {
                DiscordChannel channel = CacheHelper.DeleteChannel(data);

                if (channel.ChannelType == DiscordChannelType.DirectMessage)
                {
                    DiscordDMChannel dmChannel = (DiscordDMChannel)channel;

                    log.LogVerbose($"[CHANNEL_DELETE] Deleted dm channel with '{dmChannel.Recipient.Username}'");
                    OnChannelDeleted?.Invoke(this, new ChannelEventArgs(channel));
                }
                else
                {
                    DiscordGuildChannel deletedChannel = (DiscordGuildChannel)channel;

                    log.LogVerbose($"[CHANNEL_DELETE] Deleting channel '{deletedChannel.Name}' for guild '{deletedChannel.Guild.Name}'");
                    OnChannelDeleted?.Invoke(this, new ChannelEventArgs(deletedChannel));
                }
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[CHANNEL_DELETE] {ex.Message}");
            }
        }
        #endregion

        #region Guild Events
        void HandleGuildCreate(DiscordApiData data)
        {
            try
            {
                DiscordGuild createdGuild = CacheHelper.CreateGuild(data);

                log.LogVerbose($"[GUILD_CREATE] Received guild '{createdGuild.Name}'");
                OnGuildCreated?.Invoke(this, new GuildEventArgs(createdGuild));
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_CREATE] {ex.Message}");
            }
        }

        void HandleGuildUpdate(DiscordApiData data)
        {
            try
            {
                DiscordGuild updatedGuild = CacheHelper.UpdateGuild(data);

                OnGuildUpdated?.Invoke(this, new GuildEventArgs(updatedGuild));
                log.LogVerbose($"[GUILD_UPDATE] Updated guild '{updatedGuild.Name}'");
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_UPDATE] {ex.Message}");
            }
        }

        void HandleGuildDelete(DiscordApiData data)
        {
            try
            {
                DiscordGuild deletedGuild = CacheHelper.DeleteGuild(data);

                log.LogVerbose($"[GUILD_DELETE] Removed guild '{deletedGuild.Name}'");
                OnGuildDeleted?.Invoke(this, new GuildEventArgs(deletedGuild));
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_DELETE] {ex.Message}");
            }
        }

        void HandleGuildBanAdd(DiscordApiData data)
        {
            try
            {
                Tuple<DiscordGuild, DiscordUser> tuple = CacheHelper.AddGuildBan(data);
                DiscordGuild guild = tuple.Item1;
                DiscordUser user = tuple.Item2;

                OnGuildBanAdd?.Invoke(this, new GuildUserEventArgs(guild, user));
                log.LogVerbose($"[GUILD_BAN_ADD] Added user '{user.Username}' to ban list of guild '{guild.Name}'");
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_BAN_ADD] {ex.Message}");
            }
        }

        void HandleGuildBanRemove(DiscordApiData data)
        {
            try
            {
                Tuple<DiscordGuild, DiscordUser> tuple = CacheHelper.RemoveGuildBan(data);
                DiscordGuild guild = tuple.Item1;
                DiscordUser user = tuple.Item2;

                OnGuildBanRemove?.Invoke(this, new GuildUserEventArgs(guild, user));
                log.LogVerbose($"[GUILD_BAN_REMOVE] Removed user '{user.Username}' from ban list of guild '{guild.Name}'");
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_BAN_REMOVE] {ex.Message}");
            }
        }

        void HandleGuildEmojiUpdate(DiscordApiData data)
        {
            try
            {
                DiscordGuild guild = CacheHelper.UpdateEmoji(data);

                OnGuildEmojisUpdated?.Invoke(this, new GuildEventArgs(guild));
                log.LogVerbose($"[GUILD_EMOJI_UPDATE] Updated emojis for guild '{guild.Name}'");
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_EMOJI_UPDATE] {ex.Message}");
            }
        }

        void HandleGuildIntegrationsUpdate(DiscordApiData data)
        {
            try
            {
                DiscordIntegration integration = CacheHelper.UpdateGuildIntegrations(data);

                OnGuildIntegrationsUpdated?.Invoke(this, new IntegrationEventArgs(integration));
                log.LogVerbose($"[GUILD_INTEGRATIONS_UPDATE] Updated integration '{integration.Name}' for guild '{integration.Guild.Name}'");
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_INTEGRATIONS_UPDATE] {ex.Message}");
            }
        }

        void HandleGuildMemberAdd(DiscordApiData data)
        {
            try
            {
                DiscordGuildMember member = CacheHelper.AddGuildMember(data);

                log.LogVerbose($"[GUILD_MEMBER_ADD] Added member '{member.User.Username}' to guild '{member.Guild.Name}'");
                OnGuildMemberAdded?.Invoke(this, new GuildMemberEventArgs(member.Guild, member));
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_MEMBER_ADD] {ex.Message}");
            }
        }

        void HandleGuildMemberUpdate(DiscordApiData data)
        {
            try
            {
                DiscordGuildMember member = CacheHelper.UpdateGuildMember(data);

                OnGuildMemberUpdated?.Invoke(this, new GuildMemberEventArgs(member.Guild, member));
                log.LogVerbose($"[GUILD_MEMBER_UPDATE] Updated guild member '{member.User.Username}' for guild '{member.Guild.Name}'");
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_MEMBER_UPDATE] {ex.Message}");
            }
        }

        void HandleGuildMemberRemove(DiscordApiData data)
        {
            try
            {
                DiscordGuildMember member = CacheHelper.RemoveGuildMember(data);

                OnGuildMemberRemoved?.Invoke(this, new GuildMemberEventArgs(member.Guild, member));
                log.LogVerbose($"[GUILD_MEMBER_REMOVE] Removed guild member '{member.User.Username}' from guild '{member.Guild.Name}'");
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_MEMBER_REMOVE] {ex.Message}");
            }
        }

        void HandleGuildMembersChunk(DiscordApiData data)
        {
            try
            {
                Tuple<DiscordGuild, DiscordGuildMember[]> tuple = CacheHelper.GuildMembersChunk(data);
                DiscordGuild guild = tuple.Item1;
                DiscordGuildMember[] members = tuple.Item2;

                for (int i = 0; i < members.Length; i++)
                {
                    DiscordGuildMember member = members[i];
                    OnGuildMemberAdded?.Invoke(this, new GuildMemberEventArgs(member.Guild, member));

                    guild = member.Guild;
                }

                log.LogVerbose($"[GUILD_MEMBERS_CHUNK] Updated/Added {members.Length} members for guild '{guild.Name}'");
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_MEMBERS_CHUNK] {ex.Message}");
            }
        }

        void HandleGuildRoleCreate(DiscordApiData data)
        {
            try
            {
                Tuple<DiscordGuild, DiscordRole> tuple = CacheHelper.CreateGuildRole(data);
                DiscordGuild guild = tuple.Item1;
                DiscordRole role = tuple.Item2;

                OnGuildRoleCreated?.Invoke(this, new GuildRoleEventArgs(guild, role));
                log.LogVerbose($"[GUILD_ROLE_CREATE] Created role '{role.Name}' for guild '{guild.Name}'");
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_ROLE_CREATE] {ex.Message}");
            }
        }

        void HandleGuildRoleUpdate(DiscordApiData data)
        {
            try
            {
                Tuple<DiscordGuild, DiscordRole> tuple = CacheHelper.UpdateGuildRole(data);
                DiscordGuild guild = tuple.Item1;
                DiscordRole role = tuple.Item2;

                OnGuildRoleUpdated?.Invoke(this, new GuildRoleEventArgs(guild, role));
                log.LogVerbose($"[GUILD_ROLE_UPDATE] Updated role '{role.Name}' for guild '{guild.Name}'");
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_ROLE_UPDATE] {ex.Message}");
            }
        }

        void HandleGuildRoleDelete(DiscordApiData data)
        {
            try
            {
                Tuple<DiscordGuild, DiscordRole> tuple = CacheHelper.DeleteGuildRole(data);
                DiscordGuild guild = tuple.Item1;
                DiscordRole role = tuple.Item2;

                OnGuildRoleDeleted?.Invoke(this, new GuildRoleEventArgs(guild, role));
                log.LogVerbose($"[GUILD_ROLE_DELETE] Deleted role '{role.Name}' in guild '{guild.Name}'");
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[GUILD_ROLE_DELETE] {ex.Message}");
            }
        }
        #endregion

        #region Message Events
        void HandleMessageCreate(DiscordApiData data)
        {
            try
            {
                DiscordMessage message = CacheHelper.CreateMessage(data);

                // Guarantee a few things exist in the message
                if (message.Channel != null && message.Author != null && message.Content != null)
                    OnMessageCreated?.Invoke(this, new MessageEventArgs(message));
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[MESSAGE_CREATE] {ex.Message}");
            }
        }

        void HandleMessageUpdate(DiscordApiData data)
        {
            try
            {
                DiscordMessage message = CacheHelper.UpdateMessage(data);
                OnMessageUpdated?.Invoke(this, new MessageEventArgs(message));
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[MESSAGE_UPDATE] {ex.Message}");
            }
        }

        void HandleMessageDelete(DiscordApiData data)
        {
            try
            {
                DiscordMessage message = CacheHelper.DeleteMessage(data);

                if (message != null)
                    OnMessageDeleted?.Invoke(this, new MessageEventArgs(message));
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[MESSAGE_DELETE] {ex.Message}");
            }
        }

        void HandleMessageDeleteBulk(DiscordApiData data)
        {
            CacheHelper.DeleteMessageBulk(data);
        }
        #endregion

        #region User Events
        void HandleUserUpdate(DiscordApiData data)
        {
            try
            {
                DiscordUser user = CacheHelper.UpdateUser(data);
                log.LogVerbose($"[USER_UPDATE] Updated user '{user.Username}'");
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[USER_UPDATE] {ex.Message}");
            }
        }
        #endregion

        #region Misc Events
        void HandlePresenceUpdate(DiscordApiData data)
        {
            try
            {
                DiscordGuildMember member = CacheHelper.UpdatePresence(data);
                log.LogUnnecessary($"[PRESENCE_UPDATE] Updated presence of member '{member.User.Username}' in guild '{member.Guild.Name}'");
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[PRESENCE_UPDATE] {ex.Message}");
            }
        }

        void HandleTypingStarted(DiscordApiData data)
        {
            string channelId = data.GetString("channel_id");
            string userId = data.GetString("user_id");
            long timestamp = data.GetInt64("timestamp") ?? 0;

            DiscordChannel channel;
            if (cache.TryGet(channelId, out channel))
            {
                DiscordUser user;
                if (cache.TryGet(userId, out user))
                    OnTypingStarted?.Invoke(this, new TypingStartEventArgs(user, channel, timestamp));
                else
                    log.LogWarning($"[TYPING_STARTED] Received typing started for unknown user with id {userId}");
            }
            else
                log.LogWarning($"[TYPING_STARTED] Received typing started for unknown channel with id {channelId}");
        }

        void HandleVoiceStateUpdate(DiscordApiData data)
        {
            try
            {
                CacheHelper.UpdateVoiceState(data);
            }
            catch (DiscordApiCacheHelperException ex)
            {
                log.LogWarning($"[VOICE_STATE_UPDATE] {ex.Message}");
            }
        }
        #endregion
        #endregion

        /// <summary>
        /// Disposes of the client and disconnects from the Discord API.
        /// </summary>
        public void Dispose()
        {
            Disconnect().Wait();

            foreach (DiscordVoiceClient client in VoiceClients.Values)
                client.Dispose();

            GatewaySocket.Dispose();
        }
    }
}
