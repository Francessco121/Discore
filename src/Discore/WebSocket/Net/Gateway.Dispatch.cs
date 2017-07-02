using ConcurrentCollections;
using Discore.Voice;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    partial class Gateway
    {
        delegate void DispatchSynchronousCallback(DiscordApiData data);
        delegate Task DispatchAsynchronousCallback(DiscordApiData data);

        class DispatchCallback
        {
            public DispatchSynchronousCallback Synchronous { get; }
            public DispatchAsynchronousCallback Asynchronous { get; }

            public DispatchCallback(DispatchSynchronousCallback synchronous)
            {
                Synchronous = synchronous;
            }

            public DispatchCallback(DispatchAsynchronousCallback asynchronous)
            {
                Asynchronous = asynchronous;
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        class DispatchEventAttribute : Attribute
        {
            public string EventName { get; }

            public DispatchEventAttribute(string eventName)
            {
                EventName = eventName;
            }
        }

        #region Public Events       
        public event EventHandler<DMChannelEventArgs> OnDMChannelCreated;
        public event EventHandler<GuildChannelEventArgs> OnGuildChannelCreated;
        public event EventHandler<GuildChannelEventArgs> OnGuildChannelUpdated;
        public event EventHandler<DMChannelEventArgs> OnDMChannelRemoved;
        public event EventHandler<GuildChannelEventArgs> OnGuildChannelRemoved;


        public event EventHandler<GuildEventArgs> OnGuildCreated;
        public event EventHandler<GuildEventArgs> OnGuildUpdated;
        public event EventHandler<GuildEventArgs> OnGuildRemoved;

        public event EventHandler<GuildEventArgs> OnGuildAvailable;
        public event EventHandler<GuildEventArgs> OnGuildUnavailable;

        public event EventHandler<GuildUserEventArgs> OnGuildBanAdded;
        public event EventHandler<GuildUserEventArgs> OnGuildBanRemoved;

        public event EventHandler<GuildEventArgs> OnGuildEmojisUpdated;

        public event EventHandler<GuildIntegrationsEventArgs> OnGuildIntegrationsUpdated;

        public event EventHandler<GuildMemberEventArgs> OnGuildMemberAdded;
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberRemoved;
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberUpdated;
        public event EventHandler<GuildMemberChunkEventArgs> OnGuildMembersChunk;

        public event EventHandler<GuildRoleEventArgs> OnGuildRoleCreated;
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleUpdated;
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleDeleted;

        public event EventHandler<ChannelPinsUpdateEventArgs> OnChannelPinsUpdated;

        public event EventHandler<MessageEventArgs> OnMessageCreated;
        public event EventHandler<MessageUpdateEventArgs> OnMessageUpdated;
        public event EventHandler<MessageDeleteEventArgs> OnMessageDeleted;
        public event EventHandler<MessageReactionEventArgs> OnMessageReactionAdded;
        public event EventHandler<MessageReactionEventArgs> OnMessageReactionRemoved;
        public event EventHandler<MessageReactionRemoveAllEventArgs> OnMessageAllReactionsRemoved;

        public event EventHandler<WebhooksUpdateEventArgs> OnWebhookUpdated;

        public event EventHandler<PresenceEventArgs> OnPresenceUpdated;

        public event EventHandler<TypingStartEventArgs> OnTypingStarted;

        public event EventHandler<UserEventArgs> OnUserUpdated;
        public event EventHandler<VoiceStateEventArgs> OnVoiceStateUpdated;
        #endregion

        Dictionary<string, DispatchCallback> dispatchHandlers;

        void InitializeDispatchHandlers()
        {
            dispatchHandlers = new Dictionary<string, DispatchCallback>();

            Type taskType = typeof(Task);
            Type gatewayType = typeof(Gateway);
            Type dispatchSynchronousType = typeof(DispatchSynchronousCallback);
            Type dispatchAsynchronousType = typeof(DispatchAsynchronousCallback);

            foreach (MethodInfo method in gatewayType.GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                DispatchEventAttribute attr = method.GetCustomAttribute<DispatchEventAttribute>();
                if (attr != null)
                {
                    DispatchCallback dispatchCallback;
                    if (method.ReturnType == taskType)
                    {
                        Delegate callback = method.CreateDelegate(dispatchAsynchronousType, this);
                        dispatchCallback = new DispatchCallback((DispatchAsynchronousCallback)callback);
                    }
                    else
                    {
                        Delegate callback = method.CreateDelegate(dispatchSynchronousType, this);
                        dispatchCallback = new DispatchCallback((DispatchSynchronousCallback)callback);
                    }

                    dispatchHandlers[attr.EventName] = dispatchCallback;
                }
            }
        }

        void LogServerTrace(string prefix, DiscordApiData data)
        {
            IList<DiscordApiData> traceArray = data.GetArray("_trace");
            if (traceArray != null)
            {
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < traceArray.Count; i++)
                {
                    if (i > 0)
                        sb.Append(", ");

                    sb.Append(traceArray[i].ToString());
                }

                log.LogVerbose($"[{prefix}] trace = {sb}");
            }
        }

        [DispatchEvent("READY")]
        void HandleReadyEvent(DiscordApiData data)
        {
            // Check gateway protocol
            int protocolVersion = data.GetInteger("v").Value;
            if (protocolVersion != GATEWAY_VERSION)
                log.LogError($"[Ready] Gateway protocol mismatch! Expected v{GATEWAY_VERSION}, got {protocolVersion}.");

            // Clear the cache
            cache.Clear();

            // Get the current bot's user object
            DiscordApiData userData = data.Get("user");
            Snowflake userId = userData.GetSnowflake("id").Value;

            MutableUser user;
            if (!cache.Users.TryGetValue(userId, out user))
            {
                user = new MutableUser(userId, false, http);
                cache.Users[userId] = user;
            }

            user.Update(userData);

            shard.UserId = userId;

            log.LogInfo($"[Ready] user = {user.Username}#{user.Discriminator}");

            // Get session ID
            sessionId = data.GetString("session_id");

            // Get unavailable guilds
            foreach (DiscordApiData unavailableGuildData in data.GetArray("guilds"))
            {
                Snowflake guildId = unavailableGuildData.GetSnowflake("id").Value;

                cache.AddGuildId(guildId);
                cache.SetGuildAvailability(guildId, false);
            }

            // Get DM channels
            foreach (DiscordApiData dmChannelData in data.GetArray("private_channels"))
            {
                Snowflake channelId = dmChannelData.GetSnowflake("id").Value;
                DiscordApiData recipientData = dmChannelData.Get("recipient");
                Snowflake recipientId = recipientData.GetSnowflake("id").Value;

                MutableUser recipient;
                if (!cache.Users.TryGetValue(recipientId, out recipient))
                {
                    recipient = new MutableUser(recipientId, false, http);
                    cache.Users[recipientId] = recipient;
                }

                recipient.Update(recipientData);

                MutableDMChannel mutableDMChannel;
                if (!cache.DMChannels.TryGetValue(channelId, out mutableDMChannel))
                {
                    mutableDMChannel = new MutableDMChannel(channelId, recipient, http);
                    cache.DMChannels[channelId] = mutableDMChannel;
                }

                mutableDMChannel.Update(dmChannelData);
            }

            LogServerTrace("Ready", data);

            // Signal that the connection is ready
            handshakeCompleteEvent.Set();
        }

        [DispatchEvent("RESUMED")]
        void HandleResumedEvent(DiscordApiData data)
        {
            // Signal that the connection is ready
            handshakeCompleteEvent.Set();

            log.LogInfo("[Resumed] Successfully resumed.");
            LogServerTrace("Resumed", data);
        }

        #region Guild
        [DispatchEvent("GUILD_CREATE")]
        void HandleGuildCreateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("id").Value;

            bool wasUnavailable = !cache.IsGuildAvailable(guildId);

            // Update guild
            MutableGuild mutableGuild;
            if (!cache.Guilds.TryGetValue(guildId, out mutableGuild))
            {
                mutableGuild = new MutableGuild(guildId, http);
                cache.Guilds[guildId] = mutableGuild;
            }

            mutableGuild.Update(data);

            // Ensure the cache guildId list contains this guild (it uses a hashset so don't worry about duplicates).
            cache.AddGuildId(guildId);

            // GUILD_CREATE specifics
            // Update metadata
            cache.GuildMetadata[guildId] = new DiscordGuildMetadata(data);

            // Deserialize members
            cache.GuildMembers.Clear(guildId);
            IList<DiscordApiData> membersArray = data.GetArray("members");
            for (int i = 0; i < membersArray.Count; i++)
            {
                DiscordApiData memberData = membersArray[i];

                DiscordApiData userData = memberData.Get("user");
                Snowflake userId = userData.GetSnowflake("id").Value;

                MutableUser user;
                if (!cache.Users.TryGetValue(userId, out user))
                {
                    user = new MutableUser(userId, false, http);
                    cache.Users[userId] = user;
                }

                user.Update(userData);

                MutableGuildMember member;
                if (!cache.GuildMembers.TryGetValue(guildId, userId, out member))
                {
                    member = new MutableGuildMember(user, guildId, http);
                    cache.GuildMembers[guildId, userId] = member;
                }

                member.Update(memberData);
            }

            // Deserialize channels
            cache.ClearGuildChannels(guildId);
            IList<DiscordApiData> channelsArray = data.GetArray("channels");
            for (int i = 0; i < channelsArray.Count; i++)
            {
                DiscordApiData channelData = channelsArray[i];
                string channelType = channelData.GetString("type");

                DiscordGuildChannel channel = null;
                if (channelType == "text")
                    channel = new DiscordGuildTextChannel(http, channelData, guildId);
                else if (channelType == "voice")
                    channel = new DiscordGuildVoiceChannel(http, channelData, guildId);

                cache.AddGuildChannel(channel);
            }

            // Deserialize voice states
            cache.GuildVoiceStates.Clear(guildId);
            IList<DiscordApiData> voiceStatesArray = data.GetArray("voice_states");
            for (int i = 0; i < voiceStatesArray.Count; i++)
            {
                DiscordVoiceState state = new DiscordVoiceState(guildId, voiceStatesArray[i]);
                UpdateMemberVoiceState(state);
            }

            // Deserialize presences
            cache.GuildPresences.Clear(guildId);
            IList<DiscordApiData> presencesArray = data.GetArray("presences");
            for (int i = 0; i < presencesArray.Count; i++)
            {
                // Presence's in GUILD_CREATE do not contain full user objects,
                // so don't attempt to update them here.

                DiscordApiData presenceData = presencesArray[i];
                Snowflake userId = presenceData.LocateSnowflake("user.id").Value;

                cache.GuildPresences[guildId, userId] = new DiscordUserPresence(userId, presenceData);
            }

            // Mark the guild as available
            cache.SetGuildAvailability(guildId, true);

            // Fire event
            if (wasUnavailable)
                OnGuildAvailable?.Invoke(this, new GuildEventArgs(shard, mutableGuild.ImmutableEntity));
            else
                OnGuildCreated?.Invoke(this, new GuildEventArgs(shard, mutableGuild.ImmutableEntity));
        }

        [DispatchEvent("GUILD_UPDATE")]
        void HandleGuildUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("id").Value;

            MutableGuild mutableGuild;
            if (cache.Guilds.TryGetValue(guildId, out mutableGuild))
            {
                // Update guild
                mutableGuild.Update(data);

                // Fire event
                OnGuildUpdated?.Invoke(this, new GuildEventArgs(shard, mutableGuild.ImmutableEntity));
            }
            else
                throw new ShardCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_DELETE")]
        async Task HandleGuildDeleteEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("id").Value;
            bool unavailable = data.GetBoolean("unavailable") ?? false;

            if (unavailable)
            {
                // Tell the cache this guild is no longer available
                cache.SetGuildAvailability(guildId, false);

                if (cache.Guilds.TryGetValue(guildId, out MutableGuild mutableGuild))
                {
                    // Fire event
                    OnGuildUnavailable?.Invoke(this, new GuildEventArgs(shard, mutableGuild.ImmutableEntity));
                }
                else
                    throw new ShardCacheException($"Guild {guildId} was not in the cache! unavailable = true");
            }
            else
            {
                // Disconnect the voice connection for this guild if connected.
                if (shard.Voice.TryGetVoiceConnection(guildId, out DiscordVoiceConnection voiceConnection)
                    && voiceConnection.IsConnected)
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    cts.CancelAfter(5000);

                    await voiceConnection.DisconnectWithReasonAsync(VoiceConnectionInvalidationReason.BotRemovedFromGuild, 
                        cts.Token).ConfigureAwait(false);
                }

                // Clear all cache data related to the guild
                cache.GuildMetadata.TryRemove(guildId, out _);
                cache.GuildMembers.RemoveParent(guildId);
                cache.GuildPresences.RemoveParent(guildId);
                cache.GuildVoiceStates.RemoveParent(guildId);

                if (cache.GuildChannelIds.TryRemove(guildId, out  ConcurrentHashSet<Snowflake> channelIds))
                {
                    foreach (Snowflake channelId in channelIds)
                        cache.GuildChannels.TryRemove(channelId, out _);
                }

                // Remove the guild from cache
                cache.RemoveGuildId(guildId);
                if (cache.Guilds.TryRemove(guildId, out MutableGuild mutableGuild))
                {
                    // Ensure all references are cleared
                    mutableGuild.ClearReferences();

                    // Fire event
                    OnGuildRemoved?.Invoke(this, new GuildEventArgs(shard, mutableGuild.ImmutableEntity));
                }
                else
                    throw new ShardCacheException($"Guild {guildId} was not in the cache! unavailable = false");
            }
        }

        [DispatchEvent("GUILD_BAN_ADD")]
        void HandleGuildBanAddEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;
            DiscordApiData userData = data.Get("user");
            Snowflake userId = userData.GetSnowflake("id").Value;

            MutableUser mutableUser;
            if (!cache.Users.TryGetValue(userId, out mutableUser))
            {
                mutableUser = new MutableUser(userId, false, http);
                cache.Users[userId] = mutableUser;
            }

            mutableUser.Update(userData);

            OnGuildBanAdded?.Invoke(this, new GuildUserEventArgs(shard, guildId, mutableUser.ImmutableEntity));
        }

        [DispatchEvent("GUILD_BAN_REMOVE")]
        void HandleGuildBanRemoveEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;
            DiscordApiData userData = data.Get("user");
            Snowflake userId = userData.GetSnowflake("id").Value;

            MutableUser mutableUser;
            if (!cache.Users.TryGetValue(userId, out mutableUser))
            {
                mutableUser = new MutableUser(userId, false, http);
                cache.Users[userId] = mutableUser;
            }

            mutableUser.Update(userData);

            OnGuildBanRemoved?.Invoke(this, new GuildUserEventArgs(shard, guildId, mutableUser.ImmutableEntity));
        }

        [DispatchEvent("GUILD_EMOJIS_UPDATE")]
        void HandleGuildEmojisUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            if (cache.Guilds.TryGetValue(guildId, out MutableGuild mutableGuild))
            { 
                // Clear existing emojis
                mutableGuild.Emojis.Clear();

                // Deseralize new emojis
                IList<DiscordApiData> emojisArray = data.GetArray("emojis");
                for (int i = 0; i < emojisArray.Count; i++)
                {
                    DiscordEmoji emoji = new DiscordEmoji(emojisArray[i]);
                    mutableGuild.Emojis[emoji.Id] = emoji;
                }

                // Dirty the guild
                mutableGuild.Dirty();

                // Invoke the event
                OnGuildEmojisUpdated?.Invoke(this, new GuildEventArgs(shard, mutableGuild.ImmutableEntity));
            }
            else
                throw new ShardCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_INTEGRATIONS_UPDATE")]
        void HandleGuildIntegrationsUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            OnGuildIntegrationsUpdated?.Invoke(this, new GuildIntegrationsEventArgs(shard, guildId));
        }

        [DispatchEvent("GUILD_MEMBER_ADD")]
        void HandleGuildMemberAddEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;
            DiscordApiData userData = data.Get("user");
            Snowflake userId = userData.GetSnowflake("id").Value;

            // Get user
            MutableUser mutableUser;
            if (!cache.Users.TryGetValue(userId, out mutableUser))
            {
                mutableUser = new MutableUser(userId, false, http);
                cache.Users[userId] = mutableUser;
            }

            // Update user
            mutableUser.Update(userData);

            // Get or create member
            MutableGuildMember mutableMember;
            if (!cache.GuildMembers.TryGetValue(guildId, userId, out mutableMember))
            {
                mutableMember = new MutableGuildMember(mutableUser, guildId, http);
                cache.GuildMembers[guildId, userId] = mutableMember;
            }

            // Update member
            mutableMember.Update(data);

            // Fire event
            OnGuildMemberAdded?.Invoke(this, new GuildMemberEventArgs(shard, guildId, mutableMember.ImmutableEntity));
        }

        [DispatchEvent("GUILD_MEMBER_REMOVE")]
        void HandleGuildMemberRemoveEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;
            DiscordApiData userData = data.Get("user");
            Snowflake userId = userData.GetSnowflake("id").Value;

            // Get user
            MutableUser mutableUser;
            if (!cache.Users.TryGetValue(userId, out mutableUser))
            {
                mutableUser = new MutableUser(userId, false, http);
                cache.Users[userId] = mutableUser;
            }

            mutableUser.Update(userData);

            // Get and remove member
            if (cache.GuildMembers.TryRemove(guildId, userId, out MutableGuildMember mutableMember))
            {
                // Ensure all references are removed
                mutableMember.ClearReferences();

                // Fire event
                OnGuildMemberRemoved?.Invoke(this, new GuildMemberEventArgs(shard, guildId, mutableMember.ImmutableEntity));
            }
        }

        [DispatchEvent("GUILD_MEMBER_UPDATE")]
        void HandleGuildMemberUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;
            DiscordApiData userData = data.Get("user");
            Snowflake userId = userData.GetSnowflake("id").Value;

            // Get user
            MutableUser mutableUser;
            if (!cache.Users.TryGetValue(userId, out mutableUser))
            {
                mutableUser = new MutableUser(userId, false, http);
                cache.Users[userId] = mutableUser;
            }

            mutableUser.Update(userData);

            // Get member
            if (cache.GuildMembers.TryGetValue(guildId, userId, out MutableGuildMember mutableMember))
            {
                // Update member
                mutableMember.PartialUpdate(data);

                // Fire event
                OnGuildMemberUpdated?.Invoke(this, new GuildMemberEventArgs(shard, guildId, mutableMember.ImmutableEntity));
            }

            // It is technically valid for the member to not exist here, especially if the guild is considered large.
        }

        [DispatchEvent("GUILD_MEMBERS_CHUNK")]
        void HandleGuildMembersChunkEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            // Get every member and ensure they are cached
            IList<DiscordApiData> membersData = data.GetArray("members");
            DiscordGuildMember[] members = new DiscordGuildMember[membersData.Count];
            for (int i = 0; i < membersData.Count; i++)
            {
                DiscordApiData memberData = membersData[i];

                DiscordApiData userData = memberData.Get("user");
                Snowflake userId = userData.GetSnowflake("id").Value;

                // Get user
                MutableUser mutableUser;
                if (!cache.Users.TryGetValue(userId, out mutableUser))
                {
                    mutableUser = new MutableUser(userId, false, http);
                    cache.Users[userId] = mutableUser;
                }

                mutableUser.Update(userData);

                // Get or create member
                MutableGuildMember mutableMember;
                if (!cache.GuildMembers.TryGetValue(guildId, userId, out mutableMember))
                {
                    mutableMember = new MutableGuildMember(mutableUser, guildId, http);
                    mutableMember.Update(memberData);

                    cache.GuildMembers[guildId, userId] = mutableMember;
                }

                members[i] = mutableMember.ImmutableEntity;
            }

            // Fire event
            OnGuildMembersChunk?.Invoke(this, new GuildMemberChunkEventArgs(shard, guildId, members));
        }

        [DispatchEvent("GUILD_ROLE_CREATE")]
        void HandleGuildRoleCreateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            if (cache.Guilds.TryGetValue(guildId, out MutableGuild mutableGuild))
            {
                DiscordApiData roleData = data.Get("role");
                DiscordRole role = new DiscordRole(http, guildId, roleData);

                mutableGuild.Roles[role.Id] = role;
                mutableGuild.Dirty();

                OnGuildRoleCreated?.Invoke(this, new GuildRoleEventArgs(shard, mutableGuild.ImmutableEntity, role));
            }
            else
                throw new ShardCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_ROLE_UPDATE")]
        void HandleGuildRoleUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            if (cache.Guilds.TryGetValue(guildId, out MutableGuild mutableGuild))
            {
                DiscordApiData roleData = data.Get("role");
                DiscordRole role = new DiscordRole(http, guildId, roleData);

                mutableGuild.Roles[role.Id] = role;
                mutableGuild.Dirty();

                OnGuildRoleUpdated?.Invoke(this, new GuildRoleEventArgs(shard, mutableGuild.ImmutableEntity, role));
            }
            else
                throw new ShardCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_ROLE_DELETE")]
        void HandleGuildRoleDeleteEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            if (cache.Guilds.TryGetValue(guildId, out MutableGuild mutableGuild))
            {
                Snowflake roleId = data.GetSnowflake("role_id").Value;

                if (mutableGuild.Roles.TryRemove(roleId, out DiscordRole role))
                    OnGuildRoleDeleted?.Invoke(this, new GuildRoleEventArgs(shard, mutableGuild.ImmutableEntity, role));
                else
                    throw new ShardCacheException($"Role {roleId} was not in the guild {guildId} cache!");
            }
            else
                throw new ShardCacheException($"Guild {guildId} was not in the cache!");
        }
        #endregion

        #region Channel
        [DispatchEvent("CHANNEL_CREATE")]
        void HandleChannelCreateEvent(DiscordApiData data)
        {
            Snowflake id = data.GetSnowflake("id").Value;
            bool isPrivate = data.GetBoolean("is_private") ?? false;

            if (isPrivate)
            {
                // DM channel
                DiscordApiData recipientData = data.Get("recipient");
                Snowflake recipientId = recipientData.GetSnowflake("id").Value;

                MutableUser recipient;
                if (!cache.Users.TryGetValue(recipientId, out recipient))
                {
                    recipient = new MutableUser(recipientId, false, http);
                    cache.Users[recipientId] = recipient;
                }

                recipient.Update(recipientData);

                MutableDMChannel mutableDMChannel;
                if (!cache.DMChannels.TryGetValue(id, out mutableDMChannel))
                {
                    mutableDMChannel = new MutableDMChannel(id, recipient, http);
                    cache.DMChannels[id] = mutableDMChannel;
                }

                OnDMChannelCreated?.Invoke(this, new DMChannelEventArgs(shard, mutableDMChannel.ImmutableEntity));
            }
            else
            {
                // Guild channel
                string type = data.GetString("type");
                Snowflake guildId = data.GetSnowflake("guild_id").Value;

                DiscordGuildChannel channel;

                if (type == "text")
                    channel = new DiscordGuildTextChannel(http, data, guildId);
                else if (type == "voice")
                    channel = new DiscordGuildVoiceChannel(http, data, guildId);
                else
                    throw new NotImplementedException($"Guild channel type \"{type}\" has no implementation!");

                cache.GuildChannels[id] = channel;

                OnGuildChannelCreated?.Invoke(this, new GuildChannelEventArgs(shard, guildId, channel));
            }
        }

        [DispatchEvent("CHANNEL_UPDATE")]
        void HandleChannelUpdateEvent(DiscordApiData data)
        {
            Snowflake id = data.GetSnowflake("id").Value;
            string type = data.GetString("type");
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordGuildChannel channel;

            if (type == "text")
                channel = new DiscordGuildTextChannel(http, data, guildId);
            else if (type == "voice")
                channel = new DiscordGuildVoiceChannel(http, data, guildId);
            else
                throw new NotImplementedException($"Guild channel type \"{type}\" has no implementation!");

            cache.GuildChannels[id] = channel;

            OnGuildChannelUpdated?.Invoke(this, new GuildChannelEventArgs(shard, guildId, channel));
        }

        [DispatchEvent("CHANNEL_DELETE")]
        void HandleChannelDeleteEvent(DiscordApiData data)
        {
            Snowflake id = data.GetSnowflake("id").Value;
            bool isPrivate = data.GetBoolean("is_private") ?? false;

            if (isPrivate)
            {
                // DM channel
                DiscordDMChannel dm;
                if (cache.DMChannels.TryRemove(id, out MutableDMChannel mutableDM))
                {
                    mutableDM.ClearReferences();

                    dm = mutableDM.ImmutableEntity;
                }
                else
                    dm = new DiscordDMChannel(http, data);

                OnDMChannelRemoved?.Invoke(this, new DMChannelEventArgs(shard, dm));
            }
            else
            {
                // Guild channel
                string type = data.GetString("type");
                Snowflake guildId = data.GetSnowflake("guild_id").Value;

                DiscordGuildChannel channel;

                if (type == "text")
                {
                    if (!cache.GuildChannels.TryRemove(id, out channel))
                        channel = new DiscordGuildTextChannel(http, data, guildId);
                }
                else if (type == "voice")
                {
                    if (!cache.GuildChannels.TryRemove(id, out channel))
                        channel = new DiscordGuildVoiceChannel(http, data, guildId);
                }
                else
                    throw new NotImplementedException($"Guild channel type \"{type}\" has no implementation!");

                OnGuildChannelRemoved?.Invoke(this, new GuildChannelEventArgs(shard, guildId, channel));
            }
        }

        [DispatchEvent("CHANNEL_PINS_UPDATE")]
        void HandleChannelPinsUpdateEvent(DiscordApiData data)
        {
            DateTime? lastPinTimestamp = data.GetDateTime("last_pin_timestamp");
            Snowflake channelId = data.GetSnowflake("channel_id").Value;

            OnChannelPinsUpdated?.Invoke(this, new ChannelPinsUpdateEventArgs(shard, channelId, lastPinTimestamp));
        }
        #endregion

        #region Message
        [DispatchEvent("MESSAGE_CREATE")]
        void HandleMessageCreateEvent(DiscordApiData data)
        {
            // Get author
            DiscordApiData authorData = data.Get("author");
            Snowflake authorId = authorData.GetSnowflake("id").Value;
            bool isWebhookUser = !string.IsNullOrWhiteSpace(data.GetString("webhook_id"));

            MutableUser mutableAuthor;
            if (!cache.Users.TryGetValue(authorId, out mutableAuthor))
            {
                mutableAuthor = new MutableUser(authorId, isWebhookUser, http);
                cache.Users[authorId] = mutableAuthor;
            }

            mutableAuthor.Update(authorData);

            // Get mentioned users
            IList<DiscordApiData> mentionsArray = data.GetArray("mentions");
            for (int i = 0; i < mentionsArray.Count; i++)
            {
                DiscordApiData userData = mentionsArray[i];
                Snowflake userId = userData.GetSnowflake("id").Value;

                MutableUser mutableUser;
                if (!cache.Users.TryGetValue(userId, out mutableUser))
                {
                    mutableUser = new MutableUser(userId, false, http);
                    cache.Users[userId] = mutableUser;
                }

                mutableUser.Update(userData);
            }

            // Create message
            DiscordMessage message = new DiscordMessage(http, data);

            OnMessageCreated?.Invoke(this, new MessageEventArgs(shard, message));
        }

        [DispatchEvent("MESSAGE_UPDATE")]
        void HandleMessageUpdateEvent(DiscordApiData data)
        {
            // Get author
            DiscordApiData authorData = data.Get("author");
            if (authorData != null)
            {
                Snowflake authorId = authorData.GetSnowflake("id").Value;
                bool isWebhookUser = !string.IsNullOrWhiteSpace(data.GetString("webhook_id"));

                MutableUser mutableAuthor;
                if (!cache.Users.TryGetValue(authorId, out mutableAuthor))
                {
                    mutableAuthor = new MutableUser(authorId, isWebhookUser, http);
                    cache.Users[authorId] = mutableAuthor;
                }

                mutableAuthor.Update(authorData);
            }

            // Get mentioned users
            IList<DiscordApiData> mentionsArray = data.GetArray("mentions");
            if (mentionsArray != null)
            {
                for (int i = 0; i < mentionsArray.Count; i++)
                {
                    DiscordApiData userData = mentionsArray[i];
                    Snowflake userId = userData.GetSnowflake("id").Value;

                    MutableUser mutableUser;
                    if (!cache.Users.TryGetValue(userId, out mutableUser))
                    {
                        mutableUser = new MutableUser(userId, false, http);
                        cache.Users[userId] = mutableUser;
                    }

                    mutableUser.Update(userData);
                }
            }

            // Create message
            DiscordMessage message = new DiscordMessage(http, data);

            OnMessageUpdated?.Invoke(this, new MessageUpdateEventArgs(shard, message));
        }

        [DispatchEvent("MESSAGE_DELETE")]
        void HandleMessageDeleteEvent(DiscordApiData data)
        {
            Snowflake messageId = data.GetSnowflake("id").Value;
            Snowflake channelId = data.GetSnowflake("channel_id").Value;

            OnMessageDeleted?.Invoke(this, new MessageDeleteEventArgs(shard, messageId, channelId));
        }

        [DispatchEvent("MESSAGE_DELETE_BULK")]
        void HandleMessageDeleteBulkEvent(DiscordApiData data)
        {
            Snowflake channelId = data.GetSnowflake("channel_id").Value;

            IList<DiscordApiData> idArray = data.GetArray("ids");
            for (int i = 0; i < idArray.Count; i++)
            {
                Snowflake messageId = idArray[i].ToSnowflake().Value;
                OnMessageDeleted?.Invoke(this, new MessageDeleteEventArgs(shard, messageId, channelId));
            }
        }

        [DispatchEvent("MESSAGE_REACTION_ADD")]
        void HandleMessageReactionAddEvent(DiscordApiData data)
        {
            Snowflake userId = data.GetSnowflake("user_id").Value;
            Snowflake channelId = data.GetSnowflake("channel_id").Value;
            Snowflake messageId = data.GetSnowflake("message_id").Value;
            DiscordApiData emojiData = data.Get("emoji");

            DiscordReactionEmoji emoji = new DiscordReactionEmoji(emojiData);

            OnMessageReactionAdded?.Invoke(this, new MessageReactionEventArgs(shard, messageId, channelId, userId, emoji));
        }

        [DispatchEvent("MESSAGE_REACTION_REMOVE")]
        void HandleMessageReactionRemoveEvent(DiscordApiData data)
        {
            Snowflake userId = data.GetSnowflake("user_id").Value;
            Snowflake channelId = data.GetSnowflake("channel_id").Value;
            Snowflake messageId = data.GetSnowflake("message_id").Value;
            DiscordApiData emojiData = data.Get("emoji");

            DiscordReactionEmoji emoji = new DiscordReactionEmoji(emojiData);

            OnMessageReactionRemoved?.Invoke(this, new MessageReactionEventArgs(shard, messageId, channelId, userId, emoji));
        }

        [DispatchEvent("MESSAGE_REACTION_REMOVE_ALL")]
        void HandleMessageReactionRemoveAllEvent(DiscordApiData data)
        {
            Snowflake channelId = data.GetSnowflake("channel_id").Value;
            Snowflake messageId = data.GetSnowflake("message_id").Value;

            OnMessageAllReactionsRemoved?.Invoke(this, new MessageReactionRemoveAllEventArgs(shard, messageId, channelId));
        }
        #endregion

        [DispatchEvent("WEBHOOKS_UPDATE")]
        void HandleWebhooksUpdate(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;
            Snowflake channelId = data.GetSnowflake("channel_id").Value;

            OnWebhookUpdated?.Invoke(this, new WebhooksUpdateEventArgs(shard, guildId, channelId));
        }

        [DispatchEvent("PRESENCE_UPDATE")]
        void HandlePresenceUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            // Update user
            DiscordApiData userData = data.Get("user");
            Snowflake userId = userData.GetSnowflake("id").Value;

            if (cache.Users.TryGetValue(userId, out MutableUser mutableUser))
            {
                mutableUser.PartialUpdate(userData);
            }
            else
                // Don't throw exception since we can still update everything else...
                log.LogError($"[PRESENCE_UPDATE] Failed to update user {userId}, they were not in the cache!");

            // Update presence
            DiscordUserPresence presence = new DiscordUserPresence(userId, data);
            cache.GuildPresences[guildId, userId] = presence;

            // Update member
            if (cache.GuildMembers.TryGetValue(guildId, userId, out MutableGuildMember mutableMember))
            {
                mutableMember.PartialUpdate(data);

                // Fire event
                OnPresenceUpdated?.Invoke(this, new PresenceEventArgs(shard, guildId, mutableMember.ImmutableEntity, presence));
            }

            // It is technically valid for the member to not exist here, especially if the guild is considered large.
        }

        [DispatchEvent("TYPING_START")]
        void HandleTypingStartEvent(DiscordApiData data)
        {
            Snowflake userId = data.GetSnowflake("user_id").Value;
            Snowflake channelId = data.GetSnowflake("channel_id").Value;
            int timestamp = data.GetInteger("timestamp").Value;

            OnTypingStarted?.Invoke(this, new TypingStartEventArgs(shard, userId, channelId, timestamp));
        }

        [DispatchEvent("USER_UPDATE")]
        void HandleUserUpdateEvent(DiscordApiData data)
        {
            Snowflake userId = data.GetSnowflake("id").Value;

            MutableUser mutableUser;
            if (!cache.Users.TryGetValue(userId, out mutableUser))
            {
                mutableUser = new MutableUser(userId, false, http);
                cache.Users[userId] = mutableUser;
            }

            mutableUser.Update(data);

            OnUserUpdated?.Invoke(this, new UserEventArgs(shard, mutableUser.ImmutableEntity));
        }

        #region Voice
        /// <summary>
        /// Handles updating the cache list of members connected to voice channels, as well as updating the voice state.
        /// </summary>
        void UpdateMemberVoiceState(DiscordVoiceState newState)
        {
            // Save previous state
            DiscordVoiceState previousState = cache.GuildVoiceStates[newState.GuildId, newState.UserId];

            // Update cache with new state
            cache.GuildVoiceStates[newState.GuildId, newState.UserId] = newState;

            // If previously in a voice channel that differs from the new channel (or no longer in a channel),
            // then remove this user from the voice channel user list.
            if (previousState != null && previousState.ChannelId.HasValue && previousState.ChannelId != newState.ChannelId)
            {
                shard.Voice.RemoveUserFromVoiceChannel(previousState.ChannelId.Value, newState.UserId);
            }

            // If user is now in a voice channel, add them to the user list.
            if (newState.ChannelId.HasValue)
            {
                shard.Voice.AddUserToVoiceChannel(newState.ChannelId.Value, newState.UserId);
            }
        }

        [DispatchEvent("VOICE_STATE_UPDATE")]
        async Task HandleVoiceStateUpdateEvent(DiscordApiData data)
        {
            Snowflake? guildId = data.GetSnowflake("guild_id");
            if (guildId.HasValue) // Only guild voice channels are supported so far.
            {
                Snowflake userId = data.GetSnowflake("user_id").Value;

                // Update the voice state
                DiscordVoiceState voiceState = new DiscordVoiceState(guildId.Value, data);
                UpdateMemberVoiceState(voiceState);

                // If this voice state belongs to the current bot,
                // then we need to notify the connection of the session ID.
                if (userId == shard.UserId)
                {
                    DiscordVoiceConnection connection;
                    if (shard.Voice.TryGetVoiceConnection(guildId.Value, out connection))
                    {
                        if (voiceState.ChannelId.HasValue)
                        {
                            // Notify the connection of the new state
                            await connection.OnVoiceStateUpdated(voiceState).ConfigureAwait(false);
                        }
                        else if (connection.IsConnected)
                        {
                            // The user has left the channel, so make sure they are disconnected.
                            await connection.DisconnectAsync().ConfigureAwait(false);
                        }
                    }
                }

                // Fire event
                OnVoiceStateUpdated?.Invoke(this, new VoiceStateEventArgs(shard, voiceState));
            }
            else
                throw new NotImplementedException("Non-guild voice channels are not supported yet.");
        }

        [DispatchEvent("VOICE_SERVER_UPDATE")]
        async Task HandleVoiceServerUpdateEvent(DiscordApiData data)
        {
            Snowflake? guildId = data.GetSnowflake("guild_id");
            if (guildId.HasValue) // Only guild voice channels are supported so far.
            {
                string token = data.GetString("token");
                string endpoint = data.GetString("endpoint");

                DiscordVoiceConnection connection;
                if (shard.Voice.TryGetVoiceConnection(guildId.Value, out connection))
                {
                    // Notify the connection of the server update
                    await connection.OnVoiceServerUpdated(token, endpoint).ConfigureAwait(false);
                }
                else
                    throw new ShardCacheException($"Voice connection for guild {guildId.Value} was not in the cache!");
            }
            else
                throw new NotImplementedException("Non-guild voice channels are not supported yet.");
        }
        #endregion
    }
}
