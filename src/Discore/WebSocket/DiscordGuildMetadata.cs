using System;

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

        internal DiscordGuildMetadata(DiscordApiData data)
        {
            GuildId = data.GetSnowflake("id").Value;
            IsLarge = data.GetBoolean("large") ?? false;
            JoinedAt = data.GetDateTime("joined_at").Value;
            MemberCount = data.GetInteger("member_count").Value;
        }
    }
}
