#nullable enable

using System;
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

        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null.</exception>
        public DiscordChannelMention(Snowflake id, Snowflake guildId, DiscordChannelType type, string name)
            : base(id)
        {
            GuildId = guildId;
            Type = type;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        internal DiscordChannelMention(JsonElement json)
            : base(json)
        {
            GuildId = json.GetProperty("guild_id").GetSnowflake();
            Type = (DiscordChannelType)json.GetProperty("type").GetInt32();
            Name = json.GetProperty("name").GetString()!;
        }
    }
}

#nullable restore
