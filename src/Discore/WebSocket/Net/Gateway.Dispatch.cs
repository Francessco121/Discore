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

        public event EventHandler OnReadyEvent;

        #region Public Events       
        public event EventHandler<DMChannelEventArgs> OnDMChannelCreated;
        public event EventHandler<GuildChannelEventArgs> OnGuildChannelCreated;
        public event EventHandler<GuildChannelEventArgs> OnGuildChannelUpdated;
        public event EventHandler<DMChannelEventArgs> OnDMChannelRemoved;
        public event EventHandler<GuildChannelEventArgs> OnGuildChannelRemoved;


        public event EventHandler<GuildEventArgs> OnGuildCreated;
        public event EventHandler<GuildEventArgs> OnGuildUpdated;
        public event EventHandler<GuildEventArgs> OnGuildRemoved;

        public event EventHandler<GuildEventArgs> OnGuildUnavailable;

        public event EventHandler<GuildUserEventArgs> OnGuildBanAdded;
        public event EventHandler<GuildUserEventArgs> OnGuildBanRemoved;

        public event EventHandler<GuildEventArgs> OnGuildEmojisUpdated;

        public event EventHandler<GuildEventArgs> OnGuildIntegrationsUpdated;

        public event EventHandler<GuildMemberEventArgs> OnGuildMemberAdded;
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberRemoved;
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberUpdated;
        public event EventHandler<GuildMemberChunkEventArgs> OnGuildMembersChunk;

        public event EventHandler<GuildRoleEventArgs> OnGuildRoleCreated;
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleUpdated;
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleDeleted;

        public event EventHandler<MessageEventArgs> OnMessageCreated;
        public event EventHandler<MessageUpdateEventArgs> OnMessageUpdated;
        public event EventHandler<MessageDeleteEventArgs> OnMessageDeleted;
        public event EventHandler<MessageReactionEventArgs> OnMessageReactionAdded;
        public event EventHandler<MessageReactionEventArgs> OnMessageReactionRemoved;

        public event EventHandler<GuildMemberEventArgs> OnPresenceUpdated;

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

            // Signal that the connection is ready
            gatewayReadyEvent.Set();

            OnReadyEvent?.Invoke(this, EventArgs.Empty);

            // Get the authenticated user
            DiscordApiData userData = data.Get("user");
            Snowflake id = userData.GetSnowflake("id").Value;
            // Store authenticated user in cache for immediate use
            shard.User = cache.Users.Set(new DiscordUser(userData));

            log.LogVerbose($"[Ready] user = {shard.User}");

            // Get session id
            sessionId = data.GetString("session_id");

            // Get unavailable guilds
            foreach (DiscordApiData unavailableGuildData in data.GetArray("guilds"))
            {
                DiscoreGuildCache guildCache = new DiscoreGuildCache(cache);
                guildCache.Value = new DiscordGuild(app, guildCache, unavailableGuildData);

                cache.Guilds.Set(guildCache);
            }

            // Get DM channels
            foreach (DiscordApiData dmChannelData in data.GetArray("private_channels"))
            {
                DiscordDMChannel dm = new DiscordDMChannel(cache, app, dmChannelData);
                cache.SetDMChannel(dm);
            }

            LogServerTrace("Ready", data);
        }

        [DispatchEvent("RESUMED")]
        void HandleResumedEvent(DiscordApiData data)
        {
            log.LogInfo("[Resumed] Successfully resumed.");

            LogServerTrace("Resumed", data);
        }

        #region Guild
        [DispatchEvent("GUILD_CREATE")]
        void HandleGuildCreateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("id").Value;

            DiscoreGuildCache guildCache = cache.Guilds.Get(guildId);
            if (guildCache == null)
            {
                guildCache = new DiscoreGuildCache(cache);
                guildCache.Value = new DiscordGuild(app, guildCache, data);

                cache.Guilds.Set(guildCache);
            }
            else
                guildCache.Value = new DiscordGuild(app, guildCache, data);

            guildCache.Clear();

            IList<DiscordApiData> rolesArray = data.GetArray("roles");
            for (int i = 0; i < rolesArray.Count; i++)
                guildCache.Roles.Set(new DiscordRole(app, guildId, rolesArray[i]));

            IList<DiscordApiData> emojisArray = data.GetArray("emojis");
            for (int i = 0; i < emojisArray.Count; i++)
                guildCache.Emojis.Set(new DiscordEmoji(emojisArray[i]));

            // Guild Create specifics
            IList<DiscordApiData> membersArray = data.GetArray("members");
            for (int i = 0; i < membersArray.Count; i++)
            {
                DiscordApiData memberData = membersArray[i];

                DiscordApiData userData = memberData.Get("user");
                cache.Users.Set(new DiscordUser(userData));

                DiscoreMemberCache memberCache = new DiscoreMemberCache(guildCache);
                memberCache.Value = new DiscordGuildMember(app, cache, membersArray[i], guildId);

                guildCache.Members.Set(memberCache);
            }

            IList<DiscordApiData> channelsArray = data.GetArray("channels");
            for (int i = 0; i < channelsArray.Count; i++)
            {
                DiscordApiData channelData = channelsArray[i];
                string channelType = channelData.GetString("type");

                DiscordGuildChannel channel = null;
                if (channelType == "text")
                    channel = guildCache.SetChannel(new DiscordGuildTextChannel(app, channelData, guildId));
                else if (channelType == "voice")
                {
                    DiscordGuildVoiceChannel voiceChannel = new DiscordGuildVoiceChannel(app, channelData, guildId);
                    DiscoreVoiceChannelCache channelCache = guildCache.SetChannel(voiceChannel);
                    channelCache.Clear(); // This is a GUILD_CREATE, so we need to wipe existing data.

                    channel = voiceChannel;
                }
            }

            IList<DiscordApiData> voiceStatesArray = data.GetArray("voice_states");
            for (int i = 0; i < voiceStatesArray.Count; i++)
            {
                DiscordVoiceState state = new DiscordVoiceState(cache, guildCache, voiceStatesArray[i]);
                DiscoreMemberCache memberCache;
                if (guildCache.Members.TryGetValue(state.User.Id, out memberCache))
                    UpdateMemberVoiceState(guildCache, memberCache, state);
            }

            IList<DiscordApiData> presencesArray = data.GetArray("presences");
            for (int i = 0; i < presencesArray.Count; i++)
            {
                DiscordApiData presenceData = presencesArray[i];

                Snowflake userId = presenceData.LocateSnowflake("user.id").Value;
                DiscordUserPresence presence = new DiscordUserPresence(presencesArray[i], userId);

                DiscoreMemberCache memberCache;
                if (guildCache.Members.TryGetValue(userId, out memberCache))
                    memberCache.Presence = presence;
            }

            OnGuildCreated?.Invoke(this, new GuildEventArgs(shard, guildCache.Value));
        }

        [DispatchEvent("GUILD_UPDATE")]
        void HandleGuildUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                guildCache.Value = new DiscordGuild(app, guildCache, data);

                IList<DiscordApiData> rolesArray = data.GetArray("roles");
                for (int i = 0; i < rolesArray.Count; i++)
                    guildCache.Roles.Set(new DiscordRole(app, guildId, rolesArray[i]));

                IList<DiscordApiData> emojisArray = data.GetArray("emojis");
                for (int i = 0; i < emojisArray.Count; i++)
                    guildCache.Emojis.Set(new DiscordEmoji(emojisArray[i]));

                OnGuildUpdated?.Invoke(this, new GuildEventArgs(shard, guildCache.Value));
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_DELETE")]
        void HandleGuildDeleteEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("id").Value;

            bool unavailable = data.GetBoolean("unavailable") ?? false;

            if (unavailable)
            {
                DiscoreGuildCache guildCache = cache.Guilds.Get(guildId);
                if (guildCache != null)
                {
                    guildCache.Value = guildCache.Value.UpdateUnavailable(unavailable);
                    OnGuildUnavailable?.Invoke(this, new GuildEventArgs(shard, guildCache.Value));
                }
                else
                    throw new DiscoreCacheException($"Guild {guildId} was not in the cache! unavailalbe = {unavailable}");
            }
            else
            {
                DiscoreGuildCache guildCache = cache.Guilds.Remove(guildId);
                if (guildCache != null)
                    OnGuildRemoved?.Invoke(this, new GuildEventArgs(shard, guildCache.Value));
                else
                    throw new DiscoreCacheException($"Guild {guildId} was not in the cache! unavailalbe = {unavailable}");
            }
        }

        [DispatchEvent("GUILD_BAN_ADD")]
        void HandleGuildBanAddEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordApiData userData = data.Get("user");
            DiscordUser user = cache.Users.Set(new DiscordUser(userData));

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                OnGuildBanAdded?.Invoke(this, new GuildUserEventArgs(shard, guildCache.Value, user));
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_BAN_REMOVE")]
        void HandleGuildBanRemoveEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordApiData userData = data.Get("user");
            DiscordUser user = cache.Users.Set(new DiscordUser(userData));

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                OnGuildBanRemoved?.Invoke(this, new GuildUserEventArgs(shard, guildCache.Value, user));
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_EMOJIS_UPDATE")]
        void HandleGuildEmojisUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                // Clear existing emojis
                guildCache.Emojis.Clear();

                // Deseralize new emojis and add to cache
                IList<DiscordApiData> emojisArray = data.GetArray("emojis");
                Dictionary<Snowflake, DiscordEmoji> emojis = new Dictionary<Snowflake, DiscordEmoji>();

                for (int i = 0; i < emojisArray.Count; i++)
                {
                    DiscordEmoji emoji = new DiscordEmoji(emojisArray[i]);
                    emojis.Add(emoji.Id, emoji);

                    guildCache.Emojis.Set(emoji);
                }

                // Invoke the event
                OnGuildEmojisUpdated?.Invoke(this, new GuildEventArgs(shard, guildCache.Value));
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_INTEGRATIONS_UPDATE")]
        void HandleGuildIntegrationsUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                OnGuildIntegrationsUpdated?.Invoke(this, new GuildEventArgs(shard, guildCache.Value));
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_MEMBER_ADD")]
        void HandleGuildMemberAddEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                DiscordApiData userData = data.Get("user");
                DiscordUser user = cache.Users.Set(new DiscordUser(userData));

                DiscoreMemberCache memberCache = guildCache.Members.Get(user.Id);
                if (memberCache == null)
                {
                    memberCache = new DiscoreMemberCache(guildCache);
                    memberCache.Value = new DiscordGuildMember(app, cache, data, guildId);

                    guildCache.Members.Set(memberCache);
                }
                else
                {
                    memberCache.Clear();
                    memberCache.Value = new DiscordGuildMember(app, cache, data, guildId);
                }

                OnGuildMemberAdded?.Invoke(this, new GuildMemberEventArgs(shard, guildCache.Value, memberCache.Value));
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_MEMBER_REMOVE")]
        void HandleGuildMemberRemoveEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                DiscordApiData userData = data.Get("user");
                DiscordUser user = cache.Users.Set(new DiscordUser(userData));

                DiscordGuildMember member = guildCache.Members.Remove(user.Id)?.Value;

                if (member != null)
                    OnGuildMemberRemoved?.Invoke(this, new GuildMemberEventArgs(shard, guildCache.Value, member));
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_MEMBER_UPDATE")]
        void HandleGuildMemberUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                // Deseralize and update user in cache
                DiscordApiData userData = data.Get("user");
                DiscordUser user = cache.Users.Set(new DiscordUser(userData));

                DiscoreMemberCache memberCache;
                if (guildCache.Members.TryGetValue(user.Id, out memberCache))
                {
                    // Update the existing member and replace cache value
                    DiscordGuildMember newMember = memberCache.Value.PartialUpdate(data);
                    memberCache.Value = newMember;

                    OnGuildMemberUpdated?.Invoke(this, new GuildMemberEventArgs(shard, guildCache.Value, memberCache.Value));
                }
                else
                    throw new DiscoreCacheException($"Member {user.Id} was not in the guild {guildId} cache!");
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_MEMBERS_CHUNK")]
        void HandleGuildMembersChunkEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                IList<DiscordApiData> membersData = data.GetArray("members");
                DiscordGuildMember[] members = new DiscordGuildMember[membersData.Count];
                for (int i = 0; i < membersData.Count; i++)
                {
                    DiscordApiData memberData = membersData[i];
                    DiscordApiData userData = memberData.Get("user");
                    DiscordUser user = cache.Users.Set(new DiscordUser(userData));

                    Snowflake memberId = user.Id;

                    DiscoreMemberCache memberCache;
                    if (!guildCache.Members.TryGetValue(memberId, out memberCache))
                    {
                        memberCache = new DiscoreMemberCache(guildCache);
                        memberCache.Value = new DiscordGuildMember(app, cache, memberData, guildId);

                        guildCache.Members.Set(memberCache);
                    }
                    else
                        memberCache.Value = new DiscordGuildMember(app, cache, memberData, guildId);

                    members[i] = memberCache.Value;
                }

                OnGuildMembersChunk?.Invoke(this, new GuildMemberChunkEventArgs(Shard, guildCache.Value, members));
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_ROLE_CREATE")]
        void HandleGuildRoleCreateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                DiscordApiData roleData = data.Get("role");
                DiscordRole role = guildCache.SetRole(new DiscordRole(app, guildId, roleData));

                OnGuildRoleCreated?.Invoke(this, new GuildRoleEventArgs(shard, guildCache.Value, role));
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_ROLE_UPDATE")]
        void HandleGuildRoleUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                DiscordApiData roleData = data.Get("role");
                DiscordRole role = guildCache.SetRole(new DiscordRole(app, guildId, roleData));

                OnGuildRoleUpdated?.Invoke(this, new GuildRoleEventArgs(shard, guildCache.Value, role));
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("GUILD_ROLE_DELETE")]
        void HandleGuildRoleDeleteEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                Snowflake roleId = data.GetSnowflake("role_id").Value;

                DiscordRole role = guildCache.RemoveRole(roleId);

                if (role != null)
                    OnGuildRoleDeleted?.Invoke(this, new GuildRoleEventArgs(shard, guildCache.Value, role));
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
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
                cache.Users.Set(new DiscordUser(recipientData));

                DiscordDMChannel dm = cache.SetDMChannel(new DiscordDMChannel(cache, app, data));

                OnDMChannelCreated?.Invoke(this, new DMChannelEventArgs(shard, dm));
            }
            else
            {
                // Guild channel
                string type = data.GetString("type");
                Snowflake guildId = data.GetSnowflake("guild_id").Value;

                DiscoreGuildCache guildCache;
                if (cache.Guilds.TryGetValue(guildId, out guildCache))
                {
                    DiscordGuildChannel channel = null;

                    if (type == "text")
                        channel = guildCache.SetChannel(new DiscordGuildTextChannel(app, data));
                    else if (type == "voice")
                        channel = guildCache.SetChannel(new DiscordGuildVoiceChannel(app, data)).Value;

                    if (channel != null)
                        OnGuildChannelCreated?.Invoke(this, new GuildChannelEventArgs(shard, channel));
                }
                else
                    throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
            }
        }

        [DispatchEvent("CHANNEL_UPDATE")]
        void HandleChannelUpdateEvent(DiscordApiData data)
        {
            Snowflake id = data.GetSnowflake("id").Value;
            string type = data.GetString("type");
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                DiscordGuildChannel channel = null;

                if (type == "text")
                    channel = guildCache.SetChannel(new DiscordGuildTextChannel(app, data));
                else if (type == "voice")
                    channel = guildCache.SetChannel(new DiscordGuildVoiceChannel(app, data)).Value;

                if (channel != null)
                    OnGuildChannelUpdated?.Invoke(this, new GuildChannelEventArgs(shard, channel));
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("CHANNEL_DELETE")]
        void HandleChannelDeleteEvent(DiscordApiData data)
        {
            Snowflake id = data.GetSnowflake("id").Value;
            bool isPrivate = data.GetBoolean("is_private") ?? false;

            if (isPrivate)
            {
                // DM channel
                cache.RemoveDMChannel(id);

                DiscordDMChannel dm = new DiscordDMChannel(cache, app, data);

                OnDMChannelRemoved?.Invoke(this, new DMChannelEventArgs(shard, dm));
            }
            else
            {
                // Guild channel
                string type = data.GetString("type");
                Snowflake guildId = data.GetSnowflake("guild_id").Value;

                DiscoreGuildCache guildCache;
                if (cache.Guilds.TryGetValue(guildId, out guildCache))
                {
                    DiscordGuildChannel channel = null;

                    if (type == "text")
                    {
                        channel = guildCache.RemoveTextChannel(id);

                        // Channel wasn't found anywhere in the cache, but we can recreate it.
                        if (channel == null)
                            channel = new DiscordGuildTextChannel(app, data);
                    }
                    else if (type == "voice")
                    {
                        channel = guildCache.RemoveVoiceChannel(id).Value;

                        // Channel wasn't found anywhere in the cache, but we can recreate it.
                        if (channel == null)
                            channel = new DiscordGuildVoiceChannel(app, data);
                    }

                    OnGuildChannelRemoved?.Invoke(this, new GuildChannelEventArgs(shard, channel));
                }
                else
                    throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
            }
        }
        #endregion

        #region Message
        [DispatchEvent("MESSAGE_CREATE")]
        void HandleMessageCreateEvent(DiscordApiData data)
        {
            // Get author
            DiscordApiData authorData = data.Get("author");
            bool isWebhook = !string.IsNullOrWhiteSpace(data.GetString("webhook_id"));

            cache.Users.Set(new DiscordUser(authorData, isWebhook));

            // Get mentioned users
            IList<DiscordApiData> mentionsArray = data.GetArray("mentions");
            for (int i = 0; i < mentionsArray.Count; i++)
                cache.Users.Set(new DiscordUser(mentionsArray[i]));

            // Create message
            DiscordMessage message = new DiscordMessage(cache, app, data);

            OnMessageCreated?.Invoke(this, new MessageEventArgs(shard, message));
        }

        [DispatchEvent("MESSAGE_UPDATE")]
        void HandleMessageUpdateEvent(DiscordApiData data)
        {
            // Get author
            DiscordApiData authorData = data.Get("author");
            if (authorData != null)
            {
                bool isWebhook = !string.IsNullOrWhiteSpace(data.GetString("webhook_id"));

                cache.Users.Set(new DiscordUser(authorData, isWebhook));
            }

            // Get mentioned users
            IList<DiscordApiData> mentionsArray = data.GetArray("mentions");
            if (mentionsArray != null)
            {
                for (int i = 0; i < mentionsArray.Count; i++)
                    cache.Users.Set(new DiscordUser(mentionsArray[i]));
            }

            // Create message
            DiscordMessage message = new DiscordMessage(cache, app, data);

            OnMessageUpdated?.Invoke(this, new MessageUpdateEventArgs(shard, message));
        }

        [DispatchEvent("MESSAGE_DELETE")]
        void HandleMessageDeleteEvent(DiscordApiData data)
        {
            Snowflake messageId = data.GetSnowflake("id").Value;
            Snowflake channelId = data.GetSnowflake("channel_id").Value;

            DiscordChannel channel = cache.Channels.Get(channelId);
            if (channel != null)
                OnMessageDeleted?.Invoke(this, new MessageDeleteEventArgs(shard, messageId, channel));
        }

        [DispatchEvent("MESSAGE_DELETE_BULK")]
        void HandleMessageDeleteBulkEvent(DiscordApiData data)
        {
            Snowflake channelId = data.GetSnowflake("channel_id").Value;

            DiscordChannel channel;
            if (cache.Channels.TryGetValue(channelId, out channel))
            {
                IList<DiscordApiData> idArray = data.GetArray("ids");
                for (int i = 0; i < idArray.Count; i++)
                {
                    Snowflake messageId = idArray[i].ToSnowflake().Value;
                    OnMessageDeleted?.Invoke(this, new MessageDeleteEventArgs(shard, messageId, channel));
                }
            }
            else
                throw new DiscoreCacheException($"Channel {channelId} was not in the cache!");
        }

        [DispatchEvent("MESSAGE_REACTION_ADD")]
        void HandleMessageReactionAddEvent(DiscordApiData data)
        {
            Snowflake userId = data.GetSnowflake("user_id").Value;
            DiscordUser user;
            if (cache.Users.TryGetValue(userId, out user))
            {
                Snowflake channelId = data.GetSnowflake("channel_id").Value;
                DiscordChannel channel;
                if (cache.Channels.TryGetValue(channelId, out channel))
                {
                    DiscordApiData emojiData = data.Get("emoji");
                    DiscordReactionEmoji emoji = new DiscordReactionEmoji(emojiData);

                    Snowflake messageId = data.GetSnowflake("message_id").Value;

                    OnMessageReactionAdded?.Invoke(this, new MessageReactionEventArgs(shard, messageId, channel, user, emoji));
                }
                else
                    throw new DiscoreCacheException($"Channel {channelId} was not in the cache!");
            }
            else
                throw new DiscoreCacheException($"User {userId} was not in the cache!");
        }

        [DispatchEvent("MESSAGE_REACTION_REMOVE")]
        void HandleMessageReactionRemoveEvent(DiscordApiData data)
        {
            Snowflake userId = data.GetSnowflake("user_id").Value;
            DiscordUser user;
            if (cache.Users.TryGetValue(userId, out user))
            {
                Snowflake channelId = data.GetSnowflake("channel_id").Value;
                DiscordChannel channel;
                if (cache.Channels.TryGetValue(channelId, out channel))
                {
                    DiscordApiData emojiData = data.Get("emoji");
                    DiscordReactionEmoji emoji = new DiscordReactionEmoji(emojiData);

                    Snowflake messageId = data.GetSnowflake("message_id").Value;

                    OnMessageReactionRemoved?.Invoke(this, new MessageReactionEventArgs(shard, messageId, channel, user, emoji));
                }
                else
                    throw new DiscoreCacheException($"Channel {channelId} was not in the cache!");
            }
            else
                throw new DiscoreCacheException($"User {userId} was not in the cache!");
        }
        #endregion

        [DispatchEvent("PRESENCE_UPDATE")]
        void HandlePresenceUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                DiscordApiData userData = data.Get("user");
                Snowflake memberId = userData.GetSnowflake("id").Value;

                DiscordUser user = cache.Users.Get(memberId);
                user = cache.Users.Set(user.PartialUpdate(userData));

                DiscoreMemberCache memberCache;
                if (guildCache.Members.TryGetValue(memberId, out memberCache))
                {
                    memberCache.Value = memberCache.Value.PartialUpdate(data);
                    memberCache.Presence = new DiscordUserPresence(data, memberId);

                    OnPresenceUpdated?.Invoke(this, new GuildMemberEventArgs(shard, guildCache.Value, memberCache.Value));
                }
                else
                    throw new DiscoreCacheException($"Member {memberId} was not in the guild {guildId} cache!");
            }
            else
                throw new DiscoreCacheException($"Guild {guildId} was not in the cache!");
        }

        [DispatchEvent("TYPING_START")]
        void HandleTypingStartEvent(DiscordApiData data)
        {
            Snowflake userId = data.GetSnowflake("user_id").Value;
            Snowflake channelId = data.GetSnowflake("channel_id").Value;

            DiscordUser user;
            if (cache.Users.TryGetValue(userId, out user))
            {
                DiscordChannel channel;
                if (cache.Channels.TryGetValue(channelId, out channel))
                {
                    int timestamp = data.GetInteger("timestamp").Value;

                    OnTypingStarted?.Invoke(this, new TypingStartEventArgs(shard, user, channel, timestamp));
                }
                else
                    throw new DiscoreCacheException($"Channel {channelId} was not in the cache!");
            }
            else
                throw new DiscoreCacheException($"User {userId} was not in the cache!");
        }

        [DispatchEvent("USER_UPDATE")]
        void HandleUserUpdateEvent(DiscordApiData data)
        {
            Snowflake userId = data.GetSnowflake("id").Value;
            DiscordUser user = cache.Users.Set(new DiscordUser(data));

            OnUserUpdated?.Invoke(this, new UserEventArgs(shard, user));
        }

        #region Voice
        /// <summary>
        /// Handles updating the cache list of members connected to voice channels,
        /// as well as updating the voice state.
        /// </summary>
        void UpdateMemberVoiceState(DiscoreGuildCache guildCache, DiscoreMemberCache memberCache, DiscordVoiceState newState)
        {
            // Save old state
            DiscordVoiceState previousState = memberCache.VoiceState;

            // Set new state
            memberCache.VoiceState = newState;

            DiscordGuildVoiceChannel newVoiceChannel = newState.Channel;

            if (previousState != null)
            {
                DiscordGuildVoiceChannel previousVoiceChannel = previousState.Channel;
                if (previousVoiceChannel != null && (newVoiceChannel == null || previousVoiceChannel.Id != newVoiceChannel.Id))
                {
                    // Remove user from connected members list of previous state
                    DiscoreVoiceChannelCache voiceChannelCache = guildCache.VoiceChannels.Get(previousVoiceChannel.Id);
                    voiceChannelCache.ConnectedMembers.Remove(memberCache.DictionaryId);
                }
            }

            if (newVoiceChannel != null)
            {
                // Add user to connected members list of new state
                DiscoreVoiceChannelCache voiceChannelCache = guildCache.VoiceChannels.Get(newVoiceChannel.Id);
                voiceChannelCache.ConnectedMembers.Set(memberCache);
            }
        }

        [DispatchEvent("VOICE_STATE_UPDATE")]
        async Task HandleVoiceStateUpdateEvent(DiscordApiData data)
        {
            Snowflake? guildId = data.GetSnowflake("guild_id");
            if (guildId.HasValue) // Only guild voice channels are supported so far.
            {
                Snowflake userId = data.GetSnowflake("user_id").Value;

                DiscoreGuildCache guildCache;
                if (cache.Guilds.TryGetValue(guildId.Value, out guildCache))
                {
                    DiscoreMemberCache memberCache;
                    if (guildCache.Members.TryGetValue(userId, out memberCache))
                    {
                        DiscordVoiceState newState = new DiscordVoiceState(cache, guildCache, data);
                        UpdateMemberVoiceState(guildCache, memberCache, newState);

                        if (userId == shard.User.Id)
                        {
                            // If this voice state belongs to the current authenticated user,
                            // then we need to notify the connection of the session id.
                            DiscordVoiceConnection connection;
                            if (shard.Voice.TryGetVoiceConnection(guildId.Value, out connection))
                            {
                                if (memberCache.VoiceState.IsInVoiceChannel)
                                {
                                    // Notify the connection of the new state
                                    await connection.OnVoiceStateUpdated(memberCache.VoiceState).ConfigureAwait(false);
                                }
                                else
                                {
                                    // The user has left the channel, so disconnect.
                                    await connection.DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
                                }
                                
                            }
                        }

                        // Call event
                        OnVoiceStateUpdated?.Invoke(this, new VoiceStateEventArgs(shard, newState, memberCache.Value));
                    }
                    else
                        throw new DiscoreCacheException($"Member {guildId.Value} was not in the guild {guildId.Value} cache!");
                }
                else
                    throw new DiscoreCacheException($"Guild {guildId.Value} was not in the cache!");
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
                    throw new DiscoreCacheException($"Voice connection for guild {guildId.Value} was not in the cache!");
            }
            else
                throw new NotImplementedException("Non-guild voice channels are not supported yet.");
        }
        #endregion
    }
}
