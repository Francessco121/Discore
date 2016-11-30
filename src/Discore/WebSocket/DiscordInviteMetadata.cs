using System;

namespace Discore.WebSocket
{
    public class DiscordInviteMetadata : DiscordInvite
    {
        /// <summary>
        /// Gets the user who created the invite.
        /// </summary>
        public DiscordUser Inviter { get; private set; }

        /// <summary>
        /// Gets the number of times this invite has been used.
        /// </summary>
        public int Uses { get; private set; }

        /// <summary>
        /// Gets the maximum number of times this invite can be used.
        /// </summary>
        public int MaxUses { get; private set; }

        /// <summary>
        /// Gets the duration (in seconds) after which the invite expires.
        /// </summary>
        public int MaxAge { get; private set; }

        /// <summary>
        /// Gets whether this invite only grants temporary membership.
        /// </summary>
        public bool IsTemporary { get; private set; }

        /// <summary>
        /// Gets the date/time this invite was created.
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Gets whether this invite has been revoked.
        /// </summary>
        public bool IsRevoked { get; private set; }

        Shard shard;

        internal DiscordInviteMetadata(Shard shard)
            : base(shard)
        {
            this.shard = shard;
        }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            DiscordApiData inviterData = data.Get("inviter");
            if (inviterData != null)
            {
                Snowflake inviterId = inviterData.GetSnowflake("id").Value;
                Inviter = shard.Users.Edit(inviterId, () => new DiscordUser(), u => u.Update(inviterData));
            }

            Uses = data.GetInteger("uses") ?? Uses;
            MaxUses = data.GetInteger("max_uses") ?? MaxUses;
            MaxAge = data.GetInteger("max_age") ?? MaxAge;
            IsTemporary = data.GetBoolean("temporary") ?? IsTemporary;
            CreatedAt = data.GetDateTime("created_at") ?? CreatedAt;
            IsRevoked = data.GetBoolean("revoked") ?? IsRevoked;
        }
    }
}
