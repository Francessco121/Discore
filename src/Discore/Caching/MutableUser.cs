using System.Text.Json;

namespace Discore.Caching
{
    class MutableUser : MutableEntity<DiscordUser>
    {
        public Snowflake Id { get; }
        public bool IsWebhookUser { get; }

        public string? Username { get; private set; }
        public string? Discriminator { get; private set; }
        public DiscordCdnUrl? Avatar { get; private set; }
        public bool IsBot { get; private set; }
        public bool? MfaEnabled { get; private set; }
        public bool? IsVerified { get; private set; }
        public string? Email { get; private set; }

        string? lastUsername;
        DiscordCdnUrl? lastAvatar;
        bool? lastMfaEnabled;
        bool? lastIsVerified;
        string? lastEmail;

        public MutableUser(Snowflake id, bool isWebhookUser)
        {
            Id = id;
            IsWebhookUser = isWebhookUser;
        }

        public void Update(DiscordUser user)
        {
            Username = user.Username;
            Discriminator = user.Discriminator;
            Avatar = user.Avatar;
            IsBot = user.IsBot;
            MfaEnabled = user.MfaEnabled;
            IsVerified = user.IsVerified;
            Email = user.Email;

            // To avoid causing every entity that references this user to be unncessarily
            // dirtied, check to see if any properties actually changed with this update.
            if (Changed())
                Dirty();
        }

        public void PartialUpdate(DiscordPartialUser user)
        {
            Username = user.Username ?? Username;
            Discriminator = user.Discriminator ?? Discriminator;
            Avatar = user.Avatar ?? Avatar;

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
                || lastMfaEnabled != MfaEnabled
                || lastIsVerified != IsVerified
                || lastEmail != Email;

            if (changed)
            {
                lastUsername = Username;
                lastAvatar = Avatar;
                lastMfaEnabled = MfaEnabled;
                lastIsVerified = IsVerified;
                lastEmail = Email;
            }

            return changed;
        }

        protected override DiscordUser BuildImmutableEntity()
        {
            return new DiscordUser(
                id: Id,
                username: Username!,
                discriminator: Discriminator!,
                avatar: Avatar,
                isBot: IsBot,
                mfaEnabled: MfaEnabled,
                isVerified: IsVerified,
                email: Email,
                isWebhookUser: IsWebhookUser);
        }
    }
}
