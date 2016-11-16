using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore.Net.Sockets
{
    partial class Gateway
    {
        delegate void DispatchCallback(DiscordApiData data);

        Dictionary<string, DispatchCallback> dispatchHandlers;

        void InitializeDispatchHandlers()
        {
            dispatchHandlers = new Dictionary<string, DispatchCallback>();
            dispatchHandlers["READY"] = HandleReadyEvent;
            dispatchHandlers["GUILD_CREATE"] = HandleGuildCreate;
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

            // todo: call event
        }
    }
}
