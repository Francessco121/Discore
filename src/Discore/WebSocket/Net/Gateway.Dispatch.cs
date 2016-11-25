using Discore.WebSocket.Audio;
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
        internal event EventHandler<Snowflake[]> OnGuildMembersChunk;

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
            shard.User = shard.Users.Edit(id, () => new DiscordUser(), user => user.Update(userData));

            // Get session id
            sessionId = data.GetString("session_id");

            log.LogInfo($"[Ready] session_id = {sessionId}, user = {shard.User}");

            // Get private channels
            foreach (DiscordApiData privateChannelData in data.GetArray("private_channels"))
            {
                Snowflake channelId = privateChannelData.GetSnowflake("id").Value;

                DiscordDMChannel channel = shard.DirectMessageChannels.Edit(channelId, 
                    () => new DiscordDMChannel(shard), 
                    dm => dm.Update(privateChannelData));

                shard.Channels.Set(channelId, channel);
            }

            // Get unavailable guilds
            foreach (DiscordApiData unavailableGuildData in data.GetArray("guilds"))
            {
                Snowflake guildId = unavailableGuildData.GetSnowflake("id").Value;

                DiscordGuild guild = shard.Guilds.Edit(guildId,
                    () => new DiscordGuild(shard), g => g.Update(unavailableGuildData));
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
            DiscordGuild guild = shard.Guilds.Edit(guildId, () => new DiscordGuild(shard), g => g.Update(data));

            OnGuildCreated?.Invoke(this, new GuildEventArgs(shard, guild));
        }

        void HandleGuildUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("id").Value;
            DiscordGuild guild = shard.Guilds.Edit(guildId, () => new DiscordGuild(shard), g => g.Update(data));

            OnGuildUpdated?.Invoke(this, new GuildEventArgs(shard, guild));
        }

        void HandleGuildDeleteEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("id").Value;

            bool unavailable = data.GetBoolean("unavailable") ?? false;

            if (unavailable)
            {
                DiscordGuild guild = shard.Guilds.Get(guildId);
                if (guild != null)
                {
                    guild.Update(data);
                    OnGuildUnavailable?.Invoke(this, new GuildEventArgs(shard, guild));
                }
            }
            else
            {
                DiscordGuild guild = shard.Guilds.Remove(guildId);

                if (guild != null)
                    OnGuildRemoved?.Invoke(this, new GuildEventArgs(shard, guild));
            }
        }

        void HandleGuildBanAddEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;
            Snowflake userId = data.GetSnowflake("id").Value;

            DiscordUser user = shard.Users.Edit(userId, () => new DiscordUser(), u => u.Update(data));

            DiscordGuild guild;
            if (shard.Guilds.TryGetValue(guildId, out guild))
            {
                OnGuildBanAdded?.Invoke(this, new GuildUserEventArgs(shard, guild, user));
            }
        }

        void HandleGuildBanRemoveEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;
            Snowflake userId = data.GetSnowflake("id").Value;

            DiscordUser user = shard.Users.Edit(userId, () => new DiscordUser(), u => u.Update(data));

            DiscordGuild guild;
            if (shard.Guilds.TryGetValue(guildId, out guild))
            {
                OnGuildBanRemoved?.Invoke(this, new GuildUserEventArgs(shard, guild, user));
            }
        }

        void HandleGuildEmojisUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordGuild guild;
            if (shard.Guilds.TryGetValue(guildId, out guild))
            {
                // Emojis json variable is named the same as when a GUILD_CREATE
                // event happens, so let the partial update do its magic.
                guild.Update(data);

                OnGuildEmojisUpdated?.Invoke(this, new GuildEventArgs(shard, guild));
            }
        }

        void HandleGuildIntegrationsUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordGuild guild;
            if (shard.Guilds.TryGetValue(guildId, out guild))
            {
                OnGuildIntegrationsUpdated?.Invoke(this, new GuildEventArgs(shard, guild));
            }
        }

        void HandleGuildMemberAddEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordGuild guild;
            if (shard.Guilds.TryGetValue(guildId, out guild))
            {
                Snowflake memberId = data.LocateSnowflake("user.id").Value;
                DiscordGuildMember member = guild.Members.Edit(memberId,
                    () => new DiscordGuildMember(shard, guild), m => m.Update(data));

                OnGuildMemberAdded?.Invoke(this, new GuildMemberEventArgs(shard, guild, member));
            }
        }

        void HandleGuildMemberRemoveEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordGuild guild;
            if (shard.Guilds.TryGetValue(guildId, out guild))
            {
                Snowflake memberId = data.LocateSnowflake("user.id").Value;
                DiscordGuildMember member = guild.Members.Remove(memberId);

                if (member != null)
                    OnGuildMemberRemoved?.Invoke(this, new GuildMemberEventArgs(shard, guild, member));
            }
        }

        void HandleGuildMemberUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordGuild guild;
            if (shard.Guilds.TryGetValue(guildId, out guild))
            {
                Snowflake memberId = data.LocateSnowflake("user.id").Value;
                DiscordGuildMember member = guild.Members.Get(memberId);

                if (member != null)
                {
                    // Update variables have the same names for a guild member create,
                    // let partial update magic do its thing.
                    member.Update(data);

                    OnGuildMemberUpdated?.Invoke(this, new GuildMemberEventArgs(shard, guild, member));
                }
            }
        }

        void HandleGuildMembersChunkEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordGuild guild;
            if (shard.Guilds.TryGetValue(guildId, out guild))
            {
                IList<DiscordApiData> membersData = data.GetArray("members");
                Snowflake[] ids = new Snowflake[membersData.Count];
                for (int i = 0; i < membersData.Count; i++)
                {
                    DiscordApiData memberData = membersData[i];
                    Snowflake memberId = memberData.LocateSnowflake("user.id").Value;
                    ids[i] = memberId;

                    bool memberExistedPreviously = guild.Members.ContainsKey(memberId);

                    DiscordGuildMember member = guild.Members.Edit(memberId, 
                        () => new DiscordGuildMember(shard, guild), m => m.Update(memberData));

                    if (memberExistedPreviously)
                        OnGuildMemberUpdated?.Invoke(this, new GuildMemberEventArgs(shard, guild, member));
                    else
                        OnGuildMemberAdded?.Invoke(this, new GuildMemberEventArgs(shard, guild, member));
                }

                OnGuildMembersChunk?.Invoke(this, ids);
            }
        }

        void HandleGuildRoleCreateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordGuild guild;
            if (shard.Guilds.TryGetValue(guildId, out guild))
            {
                DiscordApiData roleData = data.Get("role");
                Snowflake roleId = roleData.GetSnowflake("id").Value;

                DiscordRole role = guild.Roles.Edit(roleId, () => new DiscordRole(), r => r.Update(roleData));
                shard.Roles.Set(roleId, role);

                OnGuildRoleCreated?.Invoke(this, new GuildRoleEventArgs(shard, guild, role));
            }
        }

        void HandleGuildRoleUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordGuild guild;
            if (shard.Guilds.TryGetValue(guildId, out guild))
            {
                DiscordApiData roleData = data.Get("role");
                Snowflake roleId = roleData.GetSnowflake("id").Value;

                DiscordRole role = guild.Roles.Edit(roleId, () => new DiscordRole(), r => r.Update(roleData));
                shard.Roles.Set(roleId, role);

                OnGuildRoleUpdated?.Invoke(this, new GuildRoleEventArgs(shard, guild, role));
            }
        }

        void HandleGuildRoleDeleteEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordGuild guild;
            if (shard.Guilds.TryGetValue(guildId, out guild))
            {
                Snowflake roleId = data.GetSnowflake("role_id").Value;

                DiscordRole role = guild.Roles.Remove(roleId);
                shard.Roles.Remove(roleId);

                if (role != null)
                    OnGuildRoleDeleted?.Invoke(this, new GuildRoleEventArgs(shard, guild, role));
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
                DiscordDMChannel dm = shard.DirectMessageChannels.Edit(id, () => new DiscordDMChannel(shard), c => c.Update(data));
                shard.Channels.Set(id, dm);

                OnDMChannelCreated?.Invoke(this, new DMChannelEventArgs(shard, dm));
            }
            else
            {
                // Guild channel
                string type = data.GetString("type");
                Snowflake guildId = data.GetSnowflake("guild_id").Value;

                DiscordGuild guild = shard.Guilds.Get(guildId);
                if (guild != null)
                {
                    DiscordGuildChannel channel = null;

                    if (type == "text")
                        channel = guild.TextChannels.Edit(id, () => new DiscordGuildTextChannel(shard, guild), c => c.Update(data));
                    else if (type == "voice")
                        channel = guild.VoiceChannels.Edit(id, () => new DiscordGuildVoiceChannel(shard, guild), c => c.Update(data));

                    if (channel != null)
                    {
                        guild.Channels.Set(id, channel);
                        shard.Channels.Set(id, channel);

                        OnGuildChannelCreated?.Invoke(this, new GuildChannelEventArgs(shard, channel));
                    }
                }
            }
        }

        void HandleChannelUpdateEvent(DiscordApiData data)
        {
            Snowflake id = data.GetSnowflake("id").Value;
            string type = data.GetString("type");
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordGuild guild = shard.Guilds.Get(guildId);
            if (guild != null)
            {
                DiscordGuildChannel channel = null;

                if (type == "text")
                    channel = guild.TextChannels.Edit(id, () => new DiscordGuildTextChannel(shard, guild), c => c.Update(data));
                else if (type == "voice")
                    channel = guild.VoiceChannels.Edit(id, () => new DiscordGuildVoiceChannel(shard, guild), c => c.Update(data));

                if (channel != null)
                {
                    guild.Channels.Set(id, channel);
                    shard.Channels.Set(id, channel);

                    OnGuildChannelUpdated?.Invoke(this, new GuildChannelEventArgs(shard, channel));
                }
            }
        }

        void HandleChannelDeleteEvent(DiscordApiData data)
        {
            Snowflake id = data.GetSnowflake("id").Value;
            bool isPrivate = data.GetBoolean("is_private") ?? false;

            if (isPrivate)
            {
                // DM channel
                shard.DirectMessageChannels.Remove(id);
                shard.Channels.Remove(id);

                DiscordDMChannel dm = new DiscordDMChannel(shard);
                dm.Update(data);

                OnDMChannelRemoved?.Invoke(this, new DMChannelEventArgs(shard, dm));
            }
            else
            {
                // Guild channel
                string type = data.GetString("type");
                Snowflake guildId = data.GetSnowflake("guild_id").Value;

                DiscordGuild guild = shard.Guilds.Get(guildId);
                if (guild != null)
                {
                    DiscordGuildChannel channel = null;

                    channel = guild.Channels.Remove(id) ?? channel;
                    channel = shard.Channels.Remove(id) as DiscordGuildChannel ?? channel;

                    if (type == "text")
                    {
                        channel = guild.TextChannels.Remove(id) ?? channel;

                        // Channel wasn't found anywhere in the cache, but we can recreate it.
                        if (channel == null)
                        {
                            channel = new DiscordGuildTextChannel(shard, guild);
                            channel.Update(data);
                        }
                    }
                    else if (type == "voice")
                    {
                        channel = guild.VoiceChannels.Remove(id) ?? channel;

                        // Channel wasn't found anywhere in the cache, but we can recreate it.
                        if (channel == null)
                        {
                            channel = new DiscordGuildVoiceChannel(shard, guild);
                            channel.Update(data);
                        }
                    }

                    OnGuildChannelRemoved?.Invoke(this, new GuildChannelEventArgs(shard, channel));
                }
            }
        }
        #endregion

        #region Message
        void HandleMessageCreateEvent(DiscordApiData data)
        {
            DiscordMessage message = new DiscordMessage(shard);
            message.Update(data);

            OnMessageCreated?.Invoke(this, new MessageEventArgs(shard, message));
        }

        void HandleMessageUpdateEvent(DiscordApiData data)
        {
            DiscordMessage message = new DiscordMessage(shard);
            message.Update(data);

            OnMessageUpdated?.Invoke(this, new MessageUpdateEventArgs(shard, message, data));
        }

        void HandleMessageDeleteEvent(DiscordApiData data)
        {
            Snowflake messageId = data.GetSnowflake("id").Value;
            Snowflake channelId = data.GetSnowflake("channel_id").Value;

            DiscordChannel channel = shard.Channels.Get(channelId);
            if (channel != null)
                OnMessageDeleted?.Invoke(this, new MessageDeleteEventArgs(shard, messageId, channel));
        }

        void HandleMessageDeleteBulkEvent(DiscordApiData data)
        {
            Snowflake channelId = data.GetSnowflake("channel_id").Value;

            DiscordChannel channel = shard.Channels.Get(channelId);
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
            if (shard.Users.TryGetValue(userId, out user))
            {
                Snowflake channelId = data.GetSnowflake("channel_id").Value;
                DiscordChannel channel;
                if (shard.Channels.TryGetValue(channelId, out channel))
                {
                    DiscordApiData emojiData = data.Get("emoji");
                    DiscordReactionEmoji emoji = new DiscordReactionEmoji();
                    emoji.Update(emojiData);

                    Snowflake messageId = data.GetSnowflake("message_id").Value;

                    OnMessageReactionAdded?.Invoke(this, new MessageReactionEventArgs(shard, messageId, channel, user, emoji));
                }
            }
        }

        void HandleMessageReactionRemoveEvent(DiscordApiData data)
        {
            Snowflake userId = data.GetSnowflake("user_id").Value;
            DiscordUser user;
            if (shard.Users.TryGetValue(userId, out user))
            {
                Snowflake channelId = data.GetSnowflake("channel_id").Value;
                DiscordChannel channel;
                if (shard.Channels.TryGetValue(channelId, out channel))
                {
                    DiscordApiData emojiData = data.Get("emoji");
                    DiscordReactionEmoji emoji = new DiscordReactionEmoji();
                    emoji.Update(emojiData);

                    Snowflake messageId = data.GetSnowflake("message_id").Value;

                    OnMessageReactionRemoved?.Invoke(this, 
                        new MessageReactionEventArgs(shard, messageId, channel, user, emoji));
                }
            }
        }
        #endregion

        void HandlePresenceUpdateEvent(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("guild_id").Value;

            DiscordGuild guild;
            if (shard.Guilds.TryGetValue(guildId, out guild))
            {
                Snowflake memberId = data.LocateSnowflake("user.id").Value;
                DiscordGuildMember member = guild.Members.Get(memberId);

                if (member != null)
                {
                    member.Update(data);
                    member.User.PresenceUpdate(data);

                    OnPresenceUpdated?.Invoke(this, new GuildMemberEventArgs(shard, guild, member));
                }
            }
        }

        void HandleTypingStartEvent(DiscordApiData data)
        {
            Snowflake userId = data.GetSnowflake("user_id").Value;
            Snowflake channelId = data.GetSnowflake("channel_id").Value;

            DiscordUser user = shard.Users.Get(userId);
            if (user != null)
            {
                DiscordChannel channel = shard.Channels.Get(channelId);
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
            DiscordUser user = shard.Users.Edit(userId, () => new DiscordUser(), u => u.Update(data));

            OnUserUpdated?.Invoke(this, new UserEventArgs(shard, user));
        }

        #region Voice
        void HandleVoiceStateUpdateEvent(DiscordApiData data)
        {
            Snowflake? guildId = data.GetSnowflake("guild_id");
            if (guildId.HasValue) // Only guild voice channels are supported so far.
            {
                Snowflake userId = data.GetSnowflake("user_id").Value;

                DiscordGuild guild;
                if (shard.Guilds.TryGetValue(guildId.Value, out guild))
                {
                    DiscordGuildMember member;
                    if (guild.Members.TryGetValue(userId, out member))
                    {
                        member.VoiceState.Update(data);

                        if (member.User == shard.User)
                        {
                            // If this voice state belongs to the current authenticated user,
                            // then we need to notify the connection of the session id.
                            DiscordVoiceConnection connection;
                            if (shard.VoiceConnectionsTable.TryGetValue(guildId.Value, out connection))
                            {
                                if (member.VoiceState.Channel != null)
                                {
                                    // Notify the connection of the new state
                                    connection.OnVoiceStateUpdated(member.VoiceState);
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
