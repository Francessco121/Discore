using System;
using System.Text.Json;

namespace Discore
{
    public sealed class DiscordReactionEmoji
    {
        /// <summary>
        /// Gets the ID of the emoji (if custom emoji).
        /// </summary>
        public Snowflake? Id { get; }

        /// <summary>
        /// Gets the name of the emoji.
        /// <para/>
        /// May be null if the emoji was deleted.
        /// </summary>
        public string? Name { get; }

        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null.</exception>
        public DiscordReactionEmoji(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null.</exception>
        public DiscordReactionEmoji(string name, Snowflake? id)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Id = id;
        }

        internal DiscordReactionEmoji(JsonElement json)
        {
            Id = json.GetProperty("id").GetSnowflakeOrNull();
            Name = json.GetProperty("name").GetString();
        }

        public override string ToString()
        {
            // TODO: This must be URL encoded!
            return (Id.HasValue ? $"{Name}:{Id.Value}" : Name) ?? base.ToString();
        }
    }
}
