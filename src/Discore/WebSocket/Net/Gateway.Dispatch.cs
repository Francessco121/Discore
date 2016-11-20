using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    partial class Gateway
    {
        delegate void DispatchCallback(DiscordApiData data);

        #region Events
        /// <summary>
        /// Called when a direct message channel is created/opened.
        /// </summary>
        public event EventHandler<DMChannelEventArgs> OnDMChannelCreated;
        /// <summary>
        /// Called when a (text or voice) guild channel is created.
        /// </summary>
        public event EventHandler<GuildChannelEventArgs> OnGuildChannelCreated;
        /// <summary>
        /// Called when a (text or voice) guild channel is updated.
        /// </summary>
        public event EventHandler<GuildChannelEventArgs> OnGuildChannelUpdated;
        /// <summary>
        /// Called when a direct message channel is removed/closed.
        /// </summary>
        public event EventHandler<DMChannelEventArgs> OnDMChannelRemoved;
        /// <summary>
        /// Called when a (text or voice) guild channel is removed.
        /// </summary>
        public event EventHandler<GuildChannelEventArgs> OnGuildChannelRemoved;
        /// <summary>
        /// Called when this application discovers a guild it is in or joins one.
        /// </summary>
        public event EventHandler<GuildEventArgs> OnGuildCreated;
        /// <summary>
        /// Called when a guild is updated.
        /// </summary>
        public event EventHandler<GuildEventArgs> OnGuildUpdated;
        /// <summary>
        /// Called when this application is removed from a guild.
        /// </summary>
        public event EventHandler<GuildEventArgs> OnGuildRemoved;
        /// <summary>
        /// Called when a known guild to this application becomes unavailable.
        /// This application was NOT removed from the guild.
        /// </summary>
        public event EventHandler<GuildEventArgs> OnGuildUnavailable;
        /// <summary>
        /// Called when a user is banned from a guild.
        /// </summary>
        public event EventHandler<GuildUserEventArgs> OnGuildBanAdded;
        /// <summary>
        /// Called when a user ban is removed from a guild.
        /// </summary>
        public event EventHandler<GuildUserEventArgs> OnGuildBanRemoved;
        /// <summary>
        /// Called when the emojis of a guild are updated.
        /// </summary>
        public event EventHandler<GuildEventArgs> OnGuildEmojisUpdated;
        /// <summary>
        /// Called when the integrations of a guild are updated.
        /// </summary>
        public event EventHandler<GuildEventArgs> OnGuildIntegrationsUpdated;
        /// <summary>
        /// Called when a user joins a guild.
        /// </summary>
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberAdded;
        /// <summary>
        /// Called when a user leaves or gets kicked/banned from a guild.
        /// </summary>
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberRemoved;
        /// <summary>
        /// Called when a member is updated for a specific guild.
        /// </summary>
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberUpdated;
        /// <summary>
        /// Called when a role is added to a guild.
        /// </summary>
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleCreated;
        /// <summary>
        /// Called when a guild role is updated.
        /// </summary>
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleUpdated;
        /// <summary>
        /// Called when a role is removed from a guild.
        /// </summary>
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleDeleted;
        /// <summary>
        /// Called when a message is created (either from a DM or guild text channel).
        /// </summary>
        public event EventHandler<MessageEventArgs> OnMessageCreated;
        /// <summary>
        /// Called when a message is updated.
        /// </summary>
        public event EventHandler<MessageEventArgs> OnMessageUpdated;
        /// <summary>
        /// Called when a message is deleted.
        /// </summary>
        public event EventHandler<MessageEventArgs> OnMessageDeleted;
        /// <summary>
        /// Called when the presence of a member in a guild is updated.
        /// </summary>
        public event EventHandler<GuildMemberEventArgs> OnPresenceUpdated;
        /// <summary>
        /// Called when a user starts typing.
        /// </summary>
        public event EventHandler<TypingStartEventArgs> OnTypingStarted;
        /// <summary>
        /// Called when a user is updated.
        /// </summary>
        public event EventHandler<UserEventArgs> OnUserUpdated;
        #endregion

        Dictionary<string, DispatchCallback> dispatchHandlers;

        void InitializeDispatchHandlers()
        {
            dispatchHandlers = new Dictionary<string, DispatchCallback>();
            dispatchHandlers["READY"] = HandleReadyEvent;
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
            dispatchHandlers["MESSAGE_CREATE"] = HandleMessageCreateEvent;
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
                for (int i = 0; i < membersData.Count; i++)
                {
                    DiscordApiData memberData = membersData[i];
                    Snowflake memberId = memberData.LocateSnowflake("user.id").Value;

                    bool memberExistedPreviously = guild.Members.ContainsKey(memberId);

                    DiscordGuildMember member = guild.Members.Edit(memberId, 
                        () => new DiscordGuildMember(shard, guild), m => m.Update(memberData));

                    if (memberExistedPreviously)
                        OnGuildMemberUpdated?.Invoke(this, new GuildMemberEventArgs(shard, guild, member));
                    else
                        OnGuildMemberAdded?.Invoke(this, new GuildMemberEventArgs(shard, guild, member));
                }
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

        }

        void HandleMessageDeleteEvent(DiscordApiData data)
        {
            
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
    }
}
