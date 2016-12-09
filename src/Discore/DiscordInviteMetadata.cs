using System;

namespace Discore
{
    public sealed class DiscordInviteMetadata : DiscordInvite
    {
        /// <summary>
        /// Gets the user who created the invite.
        /// </summary>
        public DiscordUser Inviter { get; }

        /// <summary>
        /// Gets the number of times this invite has been used.
        /// </summary>
        public int Uses { get; }

        /// <summary>
        /// Gets the maximum number of times this invite can be used.
        /// </summary>
        public int MaxUses { get; }

        /// <summary>
        /// Gets the duration (in seconds) after which the invite expires.
        /// </summary>
        public int MaxAge { get; }

        /// <summary>
        /// Gets whether this invite only grants temporary membership.
        /// </summary>
        public bool IsTemporary { get; }

        /// <summary>
        /// Gets the date/time this invite was created.
        /// </summary>
        public DateTime CreatedAt { get; }

        /// <summary>
        /// Gets whether this invite has been revoked.
        /// </summary>
        public bool IsRevoked { get; }

        public DiscordInviteMetadata(DiscordApiData data)
            : base(data)
        {
            DiscordApiData inviterData = data.Get("inviter");
            if (inviterData != null)
                Inviter = new DiscordUser(inviterData);

            Uses = data.GetInteger("uses").Value;
            MaxUses = data.GetInteger("max_uses").Value;
            MaxAge = data.GetInteger("max_age").Value;
            IsTemporary = data.GetBoolean("temporary").Value;
            CreatedAt = data.GetDateTime("created_at").Value;
            IsRevoked = data.GetBoolean("revoked").Value;
        }
    }
}
