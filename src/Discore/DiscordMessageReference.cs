using System.Text.Json;

namespace Discore
{
    public class DiscordMessageReference
    {
        /// <summary>
        /// Gets the ID of the originating message.
        /// </summary>
        public Snowflake? MessageId { get; }

        /// <summary>
        /// Gets the ID of the originating message's channel.
        /// </summary>
        public Snowflake ChannelId { get; }

        /// <summary>
        /// Gets the ID of the originating message's guild.
        /// </summary>
        public Snowflake? GuildId { get; }

        internal DiscordMessageReference(JsonElement json)
        {
            MessageId = json.GetPropertyOrNull("message_id")?.GetSnowflake();
            ChannelId = json.GetProperty("channel_id").GetSnowflake();
            GuildId = json.GetPropertyOrNull("guild_id")?.GetSnowflake();
        }

    }
}
