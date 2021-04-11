using System;
using System.Text.Json;

namespace Discore
{
    public sealed class DiscordInviteMetadata : DiscordInvite
    {
        // TODO: move to DiscordInvite
        /// <summary>
        /// Gets the user who created the invite.
        /// </summary>
        public DiscordUser? Inviter { get; }

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

        // TODO: it looks like this property was removed?
        /// <summary>
        /// Gets whether this invite has been revoked.
        /// </summary>
        public bool IsRevoked { get; }

        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="code"/> or <paramref name="channel"/> is null.
        /// </exception>
        public DiscordInviteMetadata(
            string code,
            DiscordInviteGuild? guild,
            DiscordInviteChannel channel,
            DiscordUser? targetUser,
            DiscordInviteTargetUserType? targetUserType,
            int? approximatePresenceCount,
            int? approximateMemberCount,
            DiscordUser? inviter, 
            int uses, 
            int maxUses, 
            int maxAge, 
            bool isTemporary, 
            DateTime createdAt,
            bool isRevoked)
            : base(code: code,
                  guild: guild,
                  channel: channel,
                  targetUser: targetUser,
                  targetUserType: targetUserType,
                  approximatePresenceCount: approximatePresenceCount,
                  approximateMemberCount: approximateMemberCount)
        {
            Inviter = inviter;
            Uses = uses;
            MaxUses = maxUses;
            MaxAge = maxAge;
            IsTemporary = isTemporary;
            CreatedAt = createdAt;
            IsRevoked = isRevoked;
        }

        internal DiscordInviteMetadata(JsonElement json)
            : base(json)
        {
            JsonElement? inviterJson = json.GetPropertyOrNull("inviter");
            Inviter = inviterJson == null ? null : new DiscordUser(inviterJson.Value, isWebhookUser: false);

            Uses = json.GetProperty("uses").GetInt32();
            MaxUses = json.GetProperty("max_uses").GetInt32();
            MaxAge = json.GetProperty("max_age").GetInt32();
            IsTemporary = json.GetProperty("temporary").GetBoolean();
            CreatedAt = json.GetProperty("created_at").GetDateTime();
            IsRevoked = json.GetPropertyOrNull("revoked")?.GetBoolean() ?? false;
        }
    }
}
