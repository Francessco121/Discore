namespace Discore
{
    public sealed class DiscordUser : DiscordIdObject
    {
        /// <summary>
        /// Gets the name of this user.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets the user's 4-digit discord-tag.
        /// </summary>
        public string Discriminator { get; }

        /// <summary>
        /// Gets user's avatar hash.
        /// </summary>
        public string Avatar { get; private set; }

        /// <summary>
        /// Gets whether this account belongs to an OAuth application.
        /// </summary>
        public bool IsBot { get; }

        /// <summary>
        /// Gets whether this account has two-factor authentication enabled.
        /// </summary>
        public bool HasTwoFactorAuth { get; private set; }

        /// <summary>
        /// Gets whether the email on this account is verified.
        /// </summary>
        public bool IsVerified { get; private set; }

        /// <summary>
        /// Gets the email (if available) of this account.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets whether this account is a webhook user
        /// </summary>
        public bool IsWebhook { get; }

        internal DiscordUser(DiscordApiData data, bool isWebhookUser = false)
            : base(data)
        {
            IsWebhook        = isWebhookUser;
            Username         = data.GetString("username");
            Discriminator    = data.GetString("discriminator");
            Avatar           = data.GetString("avatar");
            IsVerified       = data.GetBoolean("verified").GetValueOrDefault();
            Email            = data.GetString("email");
            IsBot            = data.GetBoolean("bot").GetValueOrDefault();
            HasTwoFactorAuth = data.GetBoolean("mfa_enabled").GetValueOrDefault();
        }

        internal DiscordUser PartialUpdate(DiscordApiData updateData)
        {
            DiscordUser newUser = (DiscordUser)MemberwiseClone();
            newUser.Username         = updateData.GetString("username") ?? Username;
            newUser.Avatar           = updateData.GetString("avatar") ?? Avatar;
            newUser.IsVerified       = updateData.GetBoolean("verified") ?? IsVerified;
            newUser.Email            = updateData.GetString("email") ?? Email;
            newUser.HasTwoFactorAuth = updateData.GetBoolean("mfa_enabled") ?? HasTwoFactorAuth;

            return newUser;
        }

        public override string ToString()
        {
            return Username;
        }
    }
}
