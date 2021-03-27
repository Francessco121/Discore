using System;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Discore
{
    public sealed class DiscordUser : DiscordIdEntity
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
        public DiscordCdnUrl Avatar { get; }

        /// <summary>
        /// Gets whether this account belongs to an OAuth application.
        /// </summary>
        public bool IsBot { get; }

        /// <summary>
        /// Gets whether this account has two-factor authentication enabled.
        /// </summary>
        [Obsolete("This information is not available to bots.")]
        public bool HasTwoFactorAuth { get; }

        /// <summary>
        /// Gets whether the email on this account is verified.
        /// </summary>
        [Obsolete("This information is not available to bots.")]
        public bool IsVerified { get; }

        /// <summary>
        /// Gets the email (if available) of this account.
        /// </summary>
        [Obsolete("This information is not available to bots.")]
        public string Email { get; }

        /// <summary>
        /// Gets whether this is a webhook user.
        /// </summary>
        public bool IsWebhookUser { get; }

        public DiscordUser(
            Snowflake id,
            string username, 
            string discriminator, 
            DiscordCdnUrl avatar, 
            bool isBot, 
            bool hasTwoFactorAuth, 
            bool isVerified, 
            string email, 
            bool isWebhookUser = false)
            : base(id)
        {
            Username = username;
            Discriminator = discriminator;
            Avatar = avatar;
            IsBot = isBot;
            HasTwoFactorAuth = hasTwoFactorAuth;
            IsVerified = isVerified;
            Email = email;
            IsWebhookUser = isWebhookUser;
        }

        internal DiscordUser(bool isWebhookUser, DiscordApiData data)
            : base(data)
        {
            IsWebhookUser = isWebhookUser;

            Username = data.GetString("username");
            Discriminator = data.GetString("discriminator");
            IsBot = data.GetBoolean("bot") ?? false;
            HasTwoFactorAuth = data.GetBoolean("mfa_enabled") ?? false;
            IsVerified = data.GetBoolean("verified") ?? false;
            Email = data.GetString("email");

            string avatarHash = data.GetString("avatar");
            if (avatarHash != null)
                Avatar = DiscordCdnUrl.ForUserAvatar(Id, avatarHash);
        }

        public override string ToString()
        {
            return Username;
        }
    }
}

#pragma warning restore CS0618 // Type or member is obsolete
