using System.Text.Json;

namespace Discore
{
    public class DiscordGuildWidgetSettings
    {
        /// <summary>
        /// Gets whether the widget is enabled.
        /// </summary>
        public bool Enabled { get; }
        /// <summary>
        /// Gets the widget channel ID.
        /// </summary>
        public Snowflake? ChannelId { get; }
        /// <summary>
        /// Gets the ID of the guild this widget is for.
        /// </summary>
        public Snowflake GuildId { get; }

        internal DiscordGuildWidgetSettings(JsonElement json, Snowflake guildId)
        {
            GuildId = guildId;

            Enabled = json.GetProperty("enabled").GetBoolean();
            ChannelId = json.GetPropertyOrNull("channel_id")?.GetSnowflakeOrNull();
        }
    }
}
