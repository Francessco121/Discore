using Discore.Http;

namespace Discore.WebSocket
{
    class MutableUser : MutableEntity<DiscordUser>
    {
        public Snowflake Id { get; }
        public bool IsWebhookUser { get; }

        public string Username { get; private set; }
        public string Discriminator { get; private set; }
        public string Avatar { get; private set; }
        public bool IsBot { get; private set; }
        public bool HasTwoFactorAuth { get; private set; }
        public bool IsVerified { get; private set; }
        public string Email { get; private set; }

        bool initialized;

        string lastUsername;
        string lastAvatar;
        bool lastHasTwoFactorAuth;
        bool lastIsVerified;
        string lastEmail;

        public MutableUser(Snowflake id, bool isWebhookUser, DiscordHttpClient http)
            : base(http)
        {
            Id = id;
            IsWebhookUser = isWebhookUser;
        }

        public void Update(DiscordApiData data)
        {
            Username = data.GetString("username");
            Discriminator = data.GetString("discriminator");
            Avatar = data.GetString("avatar");
            IsBot = data.GetBoolean("bot") ?? false;
            HasTwoFactorAuth = data.GetBoolean("mfa_enabled") ?? false;
            IsVerified = data.GetBoolean("verified") ?? false;
            Email = data.GetString("email");

            // To avoid causing every entity that references this user to be unncessarily
            // dirtied, check to see if any properties actually changed with this update.
            if (!initialized || Changed())
                Dirty();

            initialized = true;
        }

        public void PartialUpdate(DiscordApiData data)
        {
            Username = data.GetString("username") ?? Username;
            Avatar = data.GetString("avatar") ?? Avatar;
            HasTwoFactorAuth = data.GetBoolean("mfa_enabled") ?? HasTwoFactorAuth;
            IsVerified = data.GetBoolean("verified") ?? IsVerified;
            Email = data.GetString("email") ?? Email;

            // To avoid causing every entity that references this user to be unncessarily
            // dirtied, check to see if any properties actually changed with this update.
            if (Changed())
                Dirty();
        }

        bool Changed()
        {
            // Only check properties that can change

            bool changed = lastUsername != Username
                || lastAvatar != Avatar
                || lastHasTwoFactorAuth != HasTwoFactorAuth
                || lastIsVerified != IsVerified
                || lastEmail != Email;

            if (changed)
            {
                lastUsername = Username;
                lastAvatar = Avatar;
                lastHasTwoFactorAuth = HasTwoFactorAuth;
                lastIsVerified = IsVerified;
                lastEmail = Email;
            }

            return changed;
        }

        protected override DiscordUser BuildImmutableEntity()
        {
            return new DiscordUser(
                id: Id,
                username: Username,
                discriminator: Discriminator,
                avatar: Avatar != null ? DiscordCdnUrl.ForUserAvatar(Id, Avatar) : null,
                isBot: IsBot,
                hasTwoFactorAuth: HasTwoFactorAuth,
                isVerified: IsVerified,
                email: Email,
                isWebhookUser: IsWebhookUser);
        }
    }
}
