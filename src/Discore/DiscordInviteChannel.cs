using System;
using System.Text.Json;

namespace Discore
{
    public class DiscordInviteChannel
    {
        // TODO: Rename to Id
        /// <summary>
        /// Gets the ID of the channel this invite is for.
        /// </summary>
        public Snowflake ChannelId { get; }

        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the type of channel.
        /// </summary>
        public DiscordChannelType Type { get; }

        public DiscordInviteChannel(Snowflake channelId, string name, DiscordChannelType type)
        {
            ChannelId = channelId;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
        }

        internal DiscordInviteChannel(JsonElement json)
        {
            ChannelId = json.GetProperty("id").GetSnowflake();
            Name = json.GetProperty("name").GetString()!;
            Type = (DiscordChannelType)json.GetProperty("type").GetInt32();
        }
    }
}
