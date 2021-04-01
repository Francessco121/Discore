using System.Text.Json;

#nullable enable

namespace Discore
{
    public sealed class DiscordGuildEmbed
    {
        /// <summary>
        /// Gets whether this embed is enabled.
        /// </summary>
        public bool Enabled { get; }
        /// <summary>
        /// Gets the embed channel ID.
        /// </summary>
        public Snowflake? ChannelId { get; }
        /// <summary>
        /// Gets the ID of the guild this embed is for.
        /// </summary>
        public Snowflake GuildId { get; }

        public DiscordGuildEmbed(bool enabled, Snowflake? channelId, Snowflake guildId)
        {
            Enabled = enabled;
            ChannelId = channelId;
            GuildId = guildId;
        }

        internal DiscordGuildEmbed(JsonElement json, Snowflake guildId)
        {
            GuildId = guildId;

            Enabled = json.GetProperty("enabled").GetBoolean();
            ChannelId = json.GetProperty("channel_id").GetSnowflakeOrNull();
        }
    }
}

#nullable restore
