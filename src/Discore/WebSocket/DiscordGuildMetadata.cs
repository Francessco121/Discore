using System;
using System.Text.Json;

namespace Discore.WebSocket
{
    /// <summary>
    /// Shard-specific metadata about a guild.
    /// </summary>
    public class DiscordGuildMetadata
    {
        /// <summary>
        /// Gets the ID of the guild this metadata is for.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// Gets whether the guild is considered to be large.
        /// </summary>
        public bool IsLarge { get; }

        /// <summary>
        /// Gets the date and time that bot joined the guild.
        /// </summary>
        public DateTime JoinedAt { get; }

        /// <summary>
        /// Gets the total number of members in the guild.
        /// </summary>
        public int MemberCount { get; }

        public DiscordGuildMetadata(Snowflake guildId, bool isLarge, DateTime joinedAt, int memberCount)
        {
            GuildId = guildId;
            IsLarge = isLarge;
            JoinedAt = joinedAt;
            MemberCount = memberCount;
        }

        internal DiscordGuildMetadata(JsonElement json)
        {
            GuildId = json.GetProperty("id").GetSnowflake();
            IsLarge = json.GetPropertyOrNull("large")?.GetBoolean() ?? false;
            JoinedAt = json.GetProperty("joined_at").GetDateTime();
            MemberCount = json.GetProperty("member_count").GetInt32();
        }
    }
}
