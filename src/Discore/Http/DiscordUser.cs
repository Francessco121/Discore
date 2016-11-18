namespace Discore.Http
{
    public class DiscordUser : DiscordIdObject
    {
        public string Username { get; }

        /// <summary>
        /// Gets the user's 4-digit discord-tag.
        /// </summary>
        public string Discriminator { get; }

        /// <summary>
        /// Gets user's avatar hash.
        /// </summary>
        public string Avatar { get; }

        /// <summary>
        /// Gets whether this account belongs to an OAuth application.
        /// </summary>
        public bool? IsBot { get; }

        /// <summary>
        /// Gets whether this account has two-factor authentication enabled.
        /// </summary>
        public bool? HasTwoFactorAuth { get; }

        /// <summary>
        /// Gets whether the email on this account is verified.
        /// </summary>
        public bool? IsVerified { get; }

        /// <summary>
        /// Gets the email (if available) of this account.
        /// </summary>
        public string Email { get; }

        public DiscordUser(DiscordApiData data)
            : base(data)
        {
            Username         = data.GetString("username");
            Discriminator    = data.GetString("discriminator");
            Avatar           = data.GetString("avatar");
            IsVerified       = data.GetBoolean("verified");
            Email            = data.GetString("email");
            IsBot            = data.GetBoolean("bot");
            HasTwoFactorAuth = data.GetBoolean("mfa_enabled");
        }

        public override string ToString()
        {
            return Username;
        }
    }
}
