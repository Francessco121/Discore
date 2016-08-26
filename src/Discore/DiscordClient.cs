using Discore.Audio;
using Discore.Net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Discore
{
    public sealed class DiscordClient : IDiscordClient, IDisposable
    {
        public event EventHandler<ExceptionDispathEventArgs> OnError;
        public event EventHandler OnConnected;
        public event EventHandler<VoiceClientEventArgs> OnVoiceClientConnected;
        public event EventHandler<VoiceClientEventArgs> OnVoiceClientDisconnected;

        public event EventHandler<ChannelEventArgs> OnChannelCreated;
        public event EventHandler<ChannelEventArgs> OnChannelUpdated;
        public event EventHandler<ChannelEventArgs> OnChannelDeleted;
        public event EventHandler<GuildEventArgs> OnGuildCreated;
        public event EventHandler<GuildEventArgs> OnGuildUpdated;
        public event EventHandler<GuildEventArgs> OnGuildDeleted;
        public event EventHandler<GuildUserEventArgs> OnGuildBanAdd;
        public event EventHandler<GuildUserEventArgs> OnGuildBanRemove;
        public event EventHandler<GuildEventArgs> OnGuildEmojisUpdated;
        public event EventHandler<GuildEventArgs> OnGuildIntegrationsUpdated;
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberAdded;
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberUpdated;
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberRemoved;
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleCreated;
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleUpdated;
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleDeleted;
        public event EventHandler<MessageEventArgs> OnMessageCreated;
        public event EventHandler<MessageEventArgs> OnMessageUpdated;
        public event EventHandler<MessageEventArgs> OnMessageDeleted;
        public event EventHandler<TypingStartEventArgs> OnTypingStarted;

        public IReadOnlyList<KeyValuePair<string, DiscordGuild>> Guilds
        {
            get { return cache.GetList<DiscordGuild>(); }
        }
        public IReadOnlyList<KeyValuePair<string, DiscordUser>> Users
        {
            get { return cache.GetList<DiscordUser>(); }
        }

        public DiscordUser User { get; private set; }
        public bool IsConnected { get { return running && GatewaySocket.IsConnected; } }

        public DiscordApiCache Cache { get { return cache; } }
        public IDiscordGateway Gateway { get { return GatewaySocket; } }
        public IDiscordRestClient Rest { get { return RestClient; } }

        internal ConcurrentDictionary<DiscordGuild, DiscordVoiceClient> VoiceClients { get; }
        internal GatewaySocket GatewaySocket { get; }
        internal RestClient RestClient { get; }
        internal DiscordApiCacheHelper CacheHelper { get; }
        internal DiscordApiInfoCache ApiInfo { get; }

        ConcurrentDictionary<string, Action<DiscordApiData>> gatewayEventHandlers;

        ConcurrentQueue<ExceptionDispatchInfo> errors;
        Thread errorCheckingThread;
        bool running;

        DiscordLogger log;
        DiscordApiCache cache;

        public DiscordClient()
        {
            log = new DiscordLogger("DiscordClient");
            cache = new DiscordApiCache();
            CacheHelper = new DiscordApiCacheHelper(this, cache);
            ApiInfo = new DiscordApiInfoCache();

            GatewaySocket = new GatewaySocket(this);
            RestClient = new RestClient(this);
            errors = new ConcurrentQueue<ExceptionDispatchInfo>();
           
            VoiceClients = new ConcurrentDictionary<DiscordGuild, DiscordVoiceClient>();

            errorCheckingThread = new Thread(CheckForErrors);

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
        }

        private void Gateway_OnVoiceClientConnected(object sender, VoiceClientEventArgs e)
        {
            e.VoiceClient.OnDisposed += VoiceClient_OnDisposed;
            OnVoiceClientConnected?.Invoke(this, e);
        }

        private void VoiceClient_OnDisposed(object sender, VoiceClientEventArgs e)
        {
            e.VoiceClient.OnDisposed -= VoiceClient_OnDisposed;
            OnVoiceClientDisconnected?.Invoke(this, e);
        }

        private void Gateway_OnReadyEvent(object sender, DiscordApiData data)
        {
            DiscordApiData userData = data.Get("user");
            User = cache.AddOrUpdate(userData.GetString("id"), userData, () => { return new DiscordUser(); });

            foreach (DiscordApiData dmChannelData in data.GetArray("private_channels"))
            {
                string dmChannelId = dmChannelData.GetString("id");

                DiscordDMChannel dmChannel = cache.AddOrUpdate(dmChannelId, dmChannelData, () => { return new DiscordDMChannel(this); });
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

        public async Task<bool> Connect(string token)
        {
            if (!running)
            {
                if (await GatewaySocket.Connect(token))
                {
                    RestClient.SetToken(token);

                    running = true;
                    errorCheckingThread.Start();
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

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

        public void UpdateStatus(object game, DiscordGameType gameType = DiscordGameType.Default, int? idleSince = null)
            => UpdateStatus(game.ToString(), gameType, idleSince);
        public void UpdateStatus(string game, DiscordGameType gameType = DiscordGameType.Default, int? idleSince = null)
        {
            if (running)
                GatewaySocket.SendStatusUpdate(game, gameType, idleSince);
        }

        void CheckForErrors()
        {
            while (running)
            {
                if (errors.Count > 0)
                {
                    ExceptionDispatchInfo e;
                    if (errors.TryDequeue(out e))
                        OnError?.Invoke(this, new ExceptionDispathEventArgs(e));
                }

                Thread.Sleep(100);
            }
        }

        public void EnqueueError(Exception e)
        {
            errors.Enqueue(ExceptionDispatchInfo.Capture(e));
        }

        public bool TryGetUser(string id, out DiscordUser user)
        {
            return cache.TryGet(id, out user);
        }

        public bool TryGetGuild(string id, out DiscordGuild guild)
        {
            return cache.TryGet(id, out guild);
        }

        public bool TryGetDirectMessageChannel(string id, out DiscordDMChannel dmChannel)
        {
            return cache.TryGet(id, out dmChannel);
        }

        public bool TryGetChannel(string id, out DiscordChannel channel)
        {
            return cache.TryGet(id, out channel);
        }

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
                DiscordGuild guild = CacheHelper.UpdateGuildIntegrations(data);

                OnGuildIntegrationsUpdated?.Invoke(this, new GuildEventArgs(guild));
                log.LogVerbose($"[GUILD_INTEGRATIONS_UPDATE] Updated integrations for guild '{guild.Name}'");
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
                log.LogVerbose($"[PRESENCE_UPDATE] Updated presence of member '{member.User.Username}' in guild '{member.Guild.Name}'");
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

        public void Dispose()
        {
            Disconnect().Wait();

            foreach (DiscordVoiceClient client in VoiceClients.Values)
                client.Dispose();

            GatewaySocket.Dispose();
        }
    }
}
