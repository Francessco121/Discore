using Discore.WebSocket;

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
        public bool HasTwoFactorAuth { get; }

        /// <summary>
        /// Gets whether the email on this account is verified.
        /// </summary>
        public bool IsVerified { get; }

        /// <summary>
        /// Gets the email (if available) of this account.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Gets whether this is a webhook user.
        /// </summary>
        public bool IsWebhookUser { get; }

        internal DiscordUser(MutableUser user)
        {
            Id = user.Id;
            IsWebhookUser = user.IsWebhookUser;

            Username = user.Username;
            Discriminator = user.Discriminator;
            IsBot = user.IsBot;
            HasTwoFactorAuth = user.HasTwoFactorAuth;
            IsVerified = user.IsVerified;
            Email = user.Email;

            if (user.Avatar != null)
                Avatar = new DiscordCdnUrl(DiscordCdnUrlType.Avatar, user.Id, user.Avatar);
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
                Avatar = new DiscordCdnUrl(DiscordCdnUrlType.Avatar, Id, avatarHash);
        }

        public override string ToString()
        {
            return Username;
        }
    }
}
