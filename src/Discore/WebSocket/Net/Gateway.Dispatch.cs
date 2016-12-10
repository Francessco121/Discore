using Discore.Voice;
using System;
using System.Collections.Generic;

namespace Discore.WebSocket.Net
{
    partial class Gateway
    {
        delegate void DispatchCallback(DiscordApiData data);

        #region Events       
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
        internal event EventHandler<DiscordGuildMember[]> OnGuildMembersChunk;

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
        #endregion

        Dictionary<string, DispatchCallback> dispatchHandlers;

        void InitializeDispatchHandlers()
        {
            dispatchHandlers = new Dictionary<string, DispatchCallback>();
            dispatchHandlers["READY"] = HandleReadyEvent;
            dispatchHandlers["RESUMED"] = HandleResumedEvent;
            dispatchHandlers["GUILD_CREATE"] = HandleGuildCreateEvent;
            dispatchHandlers["GUILD_UPDATE"] = HandleGuildUpdateEvent;
            dispatchHandlers["GUILD_DELETE"] = HandleGuildDeleteEvent;
            dispatchHandlers["CHANNEL_CREATE"] = HandleChannelCreateEvent;
            dispatchHandlers["CHANNEL_UPDATE"] = HandleChannelUpdateEvent;
            dispatchHandlers["CHANNEL_DELETE"] = HandleChannelDeleteEvent;
            dispatchHandlers["GUILD_BAN_ADD"] = HandleGuildBanAddEvent;
            dispatchHandlers["GUILD_BAN_REMOVE"] = HandleGuildBanRemoveEvent;
            dispatchHandlers["GUILD_EMOJIS_UPDATE"] = HandleGuildEmojisUpdateEvent;
            dispatchHandlers["GUILD_INTERGRATIONS_UPDATE"] = HandleGuildIntegrationsUpdateEvent;
            dispatchHandlers["GUILD_MEMBER_ADD"] = HandleGuildMemberAddEvent;
            dispatchHandlers["GUILD_MEMBER_REMOVE"] = HandleGuildMemberRemoveEvent;
            dispatchHandlers["GUILD_MEMBER_UPDATE"] = HandleGuildMemberUpdateEvent;
            dispatchHandlers["GUILD_MEMBERS_CHUNK"] = HandleGuildMembersChunkEvent;
            dispatchHandlers["GUILD_ROLE_CREATE"] = HandleGuildRoleCreateEvent;
            dispatchHandlers["GUILD_ROLE_UPDATE"] = HandleGuildRoleUpdateEvent;
            dispatchHandlers["GUILD_ROLE_DELETE"] = HandleGuildRoleDeleteEvent;
            dispatchHandlers["PRESENCE_UPDATE"] = HandlePresenceUpdateEvent;
            dispatchHandlers["TYPING_START"] = HandleTypingStartEvent;
            dispatchHandlers["USER_UPDATE"] = HandleUserUpdateEvent;
            dispatchHandlers["MESSAGE_CREATE"] = HandleMessageCreateEvent;
            dispatchHandlers["MESSAGE_UPDATE"] = HandleMessageUpdateEvent;
            dispatchHandlers["MESSAGE_DELETE"] = HandleMessageDeleteEvent;
            dispatchHandlers["MESSAGE_DELETE_BULK"] = HandleMessageDeleteBulkEvent;
            dispatchHandlers["MESSAGE_REACTION_ADD"] = HandleMessageReactionAddEvent;
            dispatchHandlers["MESSAGE_REACTION_REMOVE"] = HandleMessageReactionRemoveEvent;
            dispatchHandlers["VOICE_STATE_UPDATE"] = HandleVoiceStateUpdateEvent;
            dispatchHandlers["VOICE_SERVER_UPDATE"] = HandleVoiceServerUpdateEvent;
        }

        void HandleReadyEvent(DiscordApiData data)
        {
            // Check gateway protocol
            int? protocolVersion = data.GetInteger("v");
            if (protocolVersion != GATEWAY_VERSION)
                log.LogError($"[Ready] Gateway protocol mismatch! Expected v{GATEWAY_VERSION}, got {protocolVersion}.");

            // Get the authenticated user
            DiscordApiData userData = data.Get("user");
            Snowflake id = userData.GetSnowflake("id").Value;
            // Store authenticated user in cache for immediate use
            shard.User = cache.Users.Set(new DiscordUser(userData));

            // Get session id
            sessionId = data.GetString("session_id");

            log.LogVerbose($"[Ready] session_id = {sessionId}, user = {shard.User}");

            // Get unavailable guilds
            foreach (DiscordApiData unavailableGuildData in data.GetArray("guilds"))
            {
                DiscoreGuildCache guildCache = new DiscoreGuildCache(cache);
                guildCache.Value = new DiscordGuild(unavailableGuildData);

                cache.Guilds.Set(guildCache);
            }
        }

        void HandleResumedEvent(DiscordApiData data)
        {
            log.LogInfo("[Resumed] Successfully resumed.");
        }

        #region Guild
        void HandleGuildCreateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("id").Value;

            DiscoreGuildCache guildCache = cache.Guilds.Get(guildId);
            if (guildCache == null)
            {
                guildCache = new DiscoreGuildCache(cache);
                guildCache.Value = new DiscordGuild(guildCache, data);

                cache.Guilds.Set(guildCache);
            }
            else
                guildCache.Value = new DiscordGuild(guildCache, data);

            guildCache.Clear();

            IList<DiscordApiData> rolesArray = data.GetArray("roles");
            for (int i = 0; i < rolesArray.Count; i++)
                guildCache.Roles.Set(new DiscordRole(rolesArray[i]));

            IList<DiscordApiData> emojisArray = data.GetArray("emojis");
            for (int i = 0; i < emojisArray.Count; i++)
                guildCache.Emojis.Set(new DiscordEmoji(emojisArray[i]));

            IList<DiscordApiData> membersArray = data.GetArray("members");
            for (int i = 0; i < membersArray.Count; i++)
            {
                DiscoreMemberCache memberCache = new DiscoreMemberCache(guildCache);
                memberCache.Value = new DiscordGuildMember(cache, membersArray[i], guildId);

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
                    channel = guildCache.SetChannel(new DiscordGuildVoiceChannel(app, channelData, guildId));
            }

            IList<DiscordApiData> voiceStatesArray = data.GetArray("voice_states");
            for (int i = 0; i < voiceStatesArray.Count; i++)
            {
                DiscordVoiceState state = new DiscordVoiceState(voiceStatesArray[i]);
                DiscoreMemberCache memberCache;
                if (guildCache.Members.TryGetValue(state.UserId, out memberCache))
                    memberCache.VoiceState = state;
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

        void HandleGuildUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("id").Value;

            DiscoreGuildCache guildCache = cache.Guilds.Get(guildId);
            if (guildCache != null)
            {
                guildCache = new DiscoreGuildCache(cache);
                cache.Guilds.Set(guildCache);
            }

            guildCache.Value = new DiscordGuild(guildCache, data);

            IList<DiscordApiData> rolesArray = data.GetArray("roles");
            for (int i = 0; i < rolesArray.Count; i++)
                guildCache.Roles.Set(new DiscordRole(rolesArray[i]));

            IList<DiscordApiData> emojisArray = data.GetArray("emojis");
            for (int i = 0; i < emojisArray.Count; i++)
                guildCache.Emojis.Set(new DiscordEmoji(emojisArray[i]));

            OnGuildUpdated?.Invoke(this, new GuildEventArgs(shard, guildCache.Value));
        }

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
            }
            else
            {
                DiscoreGuildCache guildCache = cache.Guilds.Remove(guildId);
                if (guildCache != null)
                    OnGuildRemoved?.Invoke(this, new GuildEventArgs(shard, guildCache.Value));
            }
        }

        void HandleGuildBanAddEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordUser user = cache.Users.Set(new DiscordUser(data));

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                OnGuildBanAdded?.Invoke(this, new GuildUserEventArgs(shard, guildCache.Value, user));
            }
        }

        void HandleGuildBanRemoveEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordUser user = cache.Users.Set(new DiscordUser(data));

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                OnGuildBanRemoved?.Invoke(this, new GuildUserEventArgs(shard, guildCache.Value, user));
            }
        }

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
        }

        void HandleGuildIntegrationsUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                OnGuildIntegrationsUpdated?.Invoke(this, new GuildEventArgs(shard, guildCache.Value));
            }
        }

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
                    memberCache.Value = new DiscordGuildMember(cache, data, guildId);

                    guildCache.Members.Set(memberCache);
                }
                else
                {
                    memberCache.Clear();
                    memberCache.Value = new DiscordGuildMember(cache, data, guildId);
                }

                OnGuildMemberAdded?.Invoke(this, new GuildMemberEventArgs(shard, guildCache.Value, memberCache.Value));
            }
        }

        void HandleGuildMemberRemoveEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                DiscordApiData userData = data.Get("user");
                DiscordUser user = cache.Users.Set(new DiscordUser(userData));

                Snowflake userId = userData.GetSnowflake("id").Value;
                DiscordGuildMember member = guildCache.Members.Remove(userId)?.Value;

                if (member != null)
                    OnGuildMemberRemoved?.Invoke(this, new GuildMemberEventArgs(shard, guildCache.Value, member));
            }
        }

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
            }
        }

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

                    bool memberExistedPreviously = guildCache.Members.ContainsKey(memberId);

                    DiscordGuildMember member = guildCache.Members.Set(new DiscoreMemberCache(guildCache)).Value;

                    members[i] = member;

                    if (memberExistedPreviously)
                        OnGuildMemberUpdated?.Invoke(this, new GuildMemberEventArgs(shard, guildCache.Value, member));
                    else
                        OnGuildMemberAdded?.Invoke(this, new GuildMemberEventArgs(shard, guildCache.Value, member));
                }

                OnGuildMembersChunk?.Invoke(this, members);
            }
        }

        void HandleGuildRoleCreateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                DiscordApiData roleData = data.Get("role");
                DiscordRole role = guildCache.SetRole(new DiscordRole(roleData));

                OnGuildRoleCreated?.Invoke(this, new GuildRoleEventArgs(shard, guildCache.Value, role));
            }
        }

        void HandleGuildRoleUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                DiscordApiData roleData = data.Get("role");
                DiscordRole role = guildCache.SetRole(new DiscordRole(roleData));

                OnGuildRoleUpdated?.Invoke(this, new GuildRoleEventArgs(shard, guildCache.Value, role));
            }
        }

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
        }
        #endregion

        #region Channel
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
                        channel = guildCache.SetChannel(new DiscordGuildVoiceChannel(app, data));

                    if (channel != null)
                        OnGuildChannelCreated?.Invoke(this, new GuildChannelEventArgs(shard, channel));
                }
            }
        }

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
                    channel = guildCache.SetChannel(new DiscordGuildVoiceChannel(app, data));

                if (channel != null)
                    OnGuildChannelUpdated?.Invoke(this, new GuildChannelEventArgs(shard, channel));
            }
        }

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
                        channel = guildCache.RemoveVoiceChannel(id);

                        // Channel wasn't found anywhere in the cache, but we can recreate it.
                        if (channel == null)
                            channel = new DiscordGuildVoiceChannel(app, data);
                    }

                    OnGuildChannelRemoved?.Invoke(this, new GuildChannelEventArgs(shard, channel));
                }
            }
        }
        #endregion

        #region Message
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

        void HandleMessageUpdateEvent(DiscordApiData data)
        {
            DiscordMessage message = new DiscordMessage(cache, app, data);

            OnMessageUpdated?.Invoke(this, new MessageUpdateEventArgs(shard, message, data));
        }

        void HandleMessageDeleteEvent(DiscordApiData data)
        {
            Snowflake messageId = data.GetSnowflake("id").Value;
            Snowflake channelId = data.GetSnowflake("channel_id").Value;

            DiscordChannel channel = cache.Channels.Get(channelId);
            if (channel != null)
                OnMessageDeleted?.Invoke(this, new MessageDeleteEventArgs(shard, messageId, channel));
        }

        void HandleMessageDeleteBulkEvent(DiscordApiData data)
        {
            Snowflake channelId = data.GetSnowflake("channel_id").Value;

            DiscordChannel channel = cache.Channels.Get(channelId);
            if (channel != null)
            {
                IList<DiscordApiData> idArray = data.GetArray("ids");
                for (int i = 0; i < idArray.Count; i++)
                {
                    Snowflake messageId = idArray[i].ToSnowflake().Value;
                    OnMessageDeleted?.Invoke(this, new MessageDeleteEventArgs(shard, messageId, channel));
                }
            }
        }

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
            }
        }

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
            }
        }
        #endregion

        void HandlePresenceUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscoreGuildCache guildCache;
            if (cache.Guilds.TryGetValue(guildId, out guildCache))
            {
                Snowflake memberId = data.LocateSnowflake("user.id").Value;
                DiscoreMemberCache memberCache = guildCache.Members.Get(memberId);

                if (memberCache != null)
                {
                    memberCache.Value = memberCache.Value.PartialUpdate(data);
                    memberCache.Presence = new DiscordUserPresence(data, memberId);

                    OnPresenceUpdated?.Invoke(this, new GuildMemberEventArgs(shard, guildCache.Value, memberCache.Value));
                }
            }
        }

        void HandleTypingStartEvent(DiscordApiData data)
        {
            Snowflake userId = data.GetSnowflake("user_id").Value;
            Snowflake channelId = data.GetSnowflake("channel_id").Value;

            DiscordUser user = cache.Users.Get(userId);
            if (user != null)
            {
                DiscordChannel channel = cache.Channels.Get(channelId);
                if (channel != null)
                {
                    long timestamp = data.GetInt64("timestamp").Value;

                    OnTypingStarted?.Invoke(this, new TypingStartEventArgs(shard, user, channel, timestamp));
                }
            }
        }

        void HandleUserUpdateEvent(DiscordApiData data)
        {
            Snowflake userId = data.GetSnowflake("id").Value;
            DiscordUser user = cache.Users.Set(new DiscordUser(data));

            OnUserUpdated?.Invoke(this, new UserEventArgs(shard, user));
        }

        #region Voice
        void HandleVoiceStateUpdateEvent(DiscordApiData data)
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
                        memberCache.VoiceState = new DiscordVoiceState(data);

                        if (userId == shard.User.Id)
                        {
                            // If this voice state belongs to the current authenticated user,
                            // then we need to notify the connection of the session id.
                            DiscordVoiceConnection connection;
                            if (shard.VoiceConnectionsTable.TryGetValue(guildId.Value, out connection))
                            {
                                if (memberCache.VoiceState.ChannelId != null)
                                {
                                    // Notify the connection of the new state
                                    connection.OnVoiceStateUpdated(memberCache.VoiceState);
                                }
                                else
                                {
                                    // The user has left the channel, so disconnect.
                                    connection.Disconnect();
                                }
                            }
                        }
                    }
                }
            }
        }

        void HandleVoiceServerUpdateEvent(DiscordApiData data)
        {
            Snowflake? guildId = data.GetSnowflake("guild_id");
            if (guildId.HasValue) // Only guild voice channels are supported so far.
            {
                string token = data.GetString("token");
                string endpoint = data.GetString("endpoint");

                DiscordVoiceConnection connection;
                if (shard.VoiceConnectionsTable.TryGetValue(guildId.Value, out connection))
                {
                    // Notify the connection of the server update
                    connection.OnVoiceServerUpdated(token, endpoint);
                }
            }
        }
        #endregion
    }
}
