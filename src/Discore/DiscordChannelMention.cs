using System.Text.Json;

namespace Discore
{
    public class DiscordChannelMention : DiscordIdEntity
    {
        /// <summary>
        /// Gets the ID of the guild containing the channel.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// Gets the channel type.
        /// </summary>
        public DiscordChannelType Type { get; }

        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        public string Name { get; }

        internal DiscordChannelMention(JsonElement json)
            : base(json)
        {
            GuildId = json.GetProperty("guild_id").GetSnowflake();
            Type = (DiscordChannelType)json.GetProperty("type").GetInt32();
            Name = json.GetProperty("name").GetString()!;
        }
    }
}
