using System;
using System.Text.Json;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Discore
{
    public class DiscordUser : DiscordIdEntity
    {
        /// <summary>
        /// Gets the name of this user.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Gets the user's 4-digit discord-tag.
        /// </summary>
        public string Discriminator { get; }

        /// <summary>
        /// Gets the user's avatar or null if the user does not have an avatar.
        /// </summary>
        public DiscordCdnUrl? Avatar { get; }

        /// <summary>
        /// Gets whether this account belongs to an OAuth application.
        /// </summary>
        public bool IsBot { get; }

        // TODO: Rename to MfaEnabled
        /// <summary>
        /// Gets whether this account has two-factor authentication enabled.
        /// <para/>
        /// Will be null if this user was retrieved by an account without access to this information.
        /// </summary>
        [Obsolete("This information is not available to bots.")]
        public bool? HasTwoFactorAuth { get; }

        /// <summary>
        /// Gets whether the email on this account is verified.
        /// <para/>
        /// Will be null if this user was retrieved by an account without access to this information.
        /// </summary>
        [Obsolete("This information is not available to bots.")]
        public bool? IsVerified { get; }

        /// <summary>
        /// Gets the email (if available) of this account.
        /// <para/>
        /// Will be null if this user was retrieved by an account without access to this information.
        /// </summary>
        [Obsolete("This information is not available to bots.")]
        public string? Email { get; }

        /// <summary>
        /// Gets whether this is a webhook user.
        /// </summary>
        public bool IsWebhookUser { get; }

        // TODO: Add system, locale, flags, premium_type, public_flags

        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="username"/> or <paramref name="discriminator"/> is null.
        /// </exception>
        public DiscordUser(
            Snowflake id,
            string username, 
            string discriminator, 
            DiscordCdnUrl? avatar, 
            bool isBot, 
            bool? hasTwoFactorAuth, 
            bool? isVerified, 
            string? email, 
            bool isWebhookUser = false)
            : base(id)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Discriminator = discriminator ?? throw new ArgumentNullException(nameof(discriminator));
            Avatar = avatar;
            IsBot = isBot;
            HasTwoFactorAuth = hasTwoFactorAuth;
            IsVerified = isVerified;
            Email = email;
            IsWebhookUser = isWebhookUser;
        }

        internal DiscordUser(JsonElement json, bool isWebhookUser)
            : base(json)
        {
            Username = json.GetProperty("username").GetString()!;
            Discriminator = json.GetProperty("discriminator").GetString()!;
            IsBot = json.GetPropertyOrNull("bot")?.GetBoolean() ?? false;
            HasTwoFactorAuth = json.GetPropertyOrNull("mfa_enabled")?.GetBoolean();
            IsVerified = json.GetPropertyOrNull("verified")?.GetBoolean();
            Email = json.GetPropertyOrNull("email")?.GetString();
            IsWebhookUser = isWebhookUser;

            string? avatarHash = json.GetPropertyOrNull("avatar")?.GetString();
            Avatar = avatarHash != null ? DiscordCdnUrl.ForUserAvatar(Id, avatarHash) : null;
        }

        public override string ToString()
        {
            return $"{Username}#{Discriminator}";
        }
    }
}

#pragma warning restore CS0618 // Type or member is obsolete
