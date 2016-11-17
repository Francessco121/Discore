using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore.Net.Sockets
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
        public event EventHandler<GuildEventArgs> OnEmojisUpdated;
        public event EventHandler<GuildEventArgs> OnGuildIntegrationsUpdated;
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberAdded;
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberRemoved;
        public event EventHandler<GuildMemberEventArgs> OnGuildMemberUpdated;
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleCreated;
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleUpdated;
        public event EventHandler<GuildRoleEventArgs> OnGuildRoleDeleted;
        public event EventHandler<MessageEventArgs> OnMessageCreated;
        public event EventHandler<MessageEventArgs> OnMessageUpdated;
        public event EventHandler<MessageEventArgs> OnMessageDeleted;
        public event EventHandler<GuildMemberEventArgs> OnPresenceUpdated;
        public event EventHandler<TypingStartEventArgs> OnTypingStarted;
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
    }
}
