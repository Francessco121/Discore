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
        public event EventHandler<GuildEventArgs> OnEmojisUpdated;
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
            dispatchHandlers["GUILD_CREATE"] = HandleGuildCreate;
            dispatchHandlers["GUILD_UPDATE"] = HandleGuildUpdate;
            dispatchHandlers["GUILD_DELETE"] = HandleGuildDelete;
            dispatchHandlers["CHANNEL_CREATE"] = HandleChannelCreate;
            dispatchHandlers["CHANNEL_UPDATE"] = HandleChannelUpdate;
            dispatchHandlers["CHANNEL_DELETE"] = HandleChannelDelete;
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

        void HandleGuildCreate(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("id").Value;
            DiscordGuild guild = shard.Guilds.Edit(guildId, () => new DiscordGuild(shard), g => g.Update(data));

            OnGuildCreated?.Invoke(this, new GuildEventArgs(shard, guild));
        }

        void HandleGuildUpdate(DiscordApiData data)
        {
            Snowflake guildId = data.GetSnowflake("id").Value;
            DiscordGuild guild = shard.Guilds.Edit(guildId, () => new DiscordGuild(shard), g => g.Update(data));

            OnGuildUpdated?.Invoke(this, new GuildEventArgs(shard, guild));
        }

        void HandleGuildDelete(DiscordApiData data)
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

        void HandleChannelCreate(DiscordApiData data)
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

        void HandleChannelUpdate(DiscordApiData data)
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

        void HandleChannelDelete(DiscordApiData data)
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
                    if (type == "text")
                        guild.TextChannels.Remove(id);
                    else if (type == "voice")
                        guild.VoiceChannels.Remove(id);

                    guild.Channels.Remove(id);
                    shard.Channels.Remove(id);
                }
            }
        }
    }
}
