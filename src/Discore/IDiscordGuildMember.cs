using System;
using System.Collections.Generic;

namespace Discore
{
    /// <summary>
    /// Common properties returned for guild members regardless of API.
    /// </summary>
    public interface IDiscordGuildMember
    {
        /// <summary>
        /// Gets the ID of the member.
        /// This is always the ID of the associated user.
        /// </summary>
        Snowflake Id { get; }

        /// <summary>
        /// Gets the ID of the guild this member is in.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// Gets the IDs of all of the roles this member has.
        /// </summary>
        public IReadOnlyList<Snowflake> RoleIds { get; }

        /// <summary>
        /// Gets the guild-wide nickname of the user.
        /// </summary>
        public string? Nickname { get; }

        /// <summary>
        /// Gets the time this member joined the guild.
        /// </summary>
        public DateTime JoinedAt { get; }

        /// <summary>
        /// Gets whether this member is deafened.
        /// </summary>
        public bool IsDeaf { get; }

        /// <summary>
        /// Gets whether this member is muted.
        /// </summary>
        public bool IsMute { get; }

        // TODO: add premium_since, avatar, communication_disabled_until
    }
}
