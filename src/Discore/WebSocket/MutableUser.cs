using System.Text.Json;

namespace Discore.WebSocket
{
    class MutableUser : MutableEntity<DiscordUser>
    {
        public Snowflake Id { get; }
        public bool IsWebhookUser { get; }

        public string? Username { get; private set; }
        public string? Discriminator { get; private set; }
        public string? Avatar { get; private set; }
        public bool IsBot { get; private set; }
        public bool? HasTwoFactorAuth { get; private set; }
        public bool? IsVerified { get; private set; }
        public string? Email { get; private set; }

        string? lastUsername;
        string? lastAvatar;
        bool? lastHasTwoFactorAuth;
        bool? lastIsVerified;
        string? lastEmail;

        public MutableUser(Snowflake id, bool isWebhookUser)
        {
            Id = id;
            IsWebhookUser = isWebhookUser;
        }

        public void Update(JsonElement json)
        {
            Username = json.GetProperty("username").GetString()!;
            Discriminator = json.GetProperty("discriminator").GetString()!;
            Avatar = json.GetProperty("avatar").GetString();
            IsBot = json.GetPropertyOrNull("bot")?.GetBoolean() ?? false;
            HasTwoFactorAuth = json.GetPropertyOrNull("mfa_enabled")?.GetBoolean();
            IsVerified = json.GetPropertyOrNull("verified")?.GetBoolean();
            Email = json.GetPropertyOrNull("email")?.GetString();

            // To avoid causing every entity that references this user to be unncessarily
            // dirtied, check to see if any properties actually changed with this update.
            if (Changed())
                Dirty();
        }

        public void PartialUpdate(JsonElement json)
        {
            Username = json.GetPropertyOrNull("username")?.GetString() ?? Username;
            Discriminator = json.GetPropertyOrNull("discriminator")?.GetString() ?? Discriminator;
            Avatar = json.GetPropertyOrNull("avatar")?.GetString() ?? Avatar;
            IsBot = json.GetPropertyOrNull("bot")?.GetBoolean() ?? IsBot;
            HasTwoFactorAuth = json.GetPropertyOrNull("mfa_enabled")?.GetBoolean() ?? HasTwoFactorAuth;
            IsVerified = json.GetPropertyOrNull("verified")?.GetBoolean() ?? IsVerified;
            Email = json.GetPropertyOrNull("email")?.GetString() ?? Email;

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
                username: Username!,
                discriminator: Discriminator!,
                avatar: Avatar != null ? DiscordCdnUrl.ForUserAvatar(Id, Avatar) : null,
                isBot: IsBot,
                hasTwoFactorAuth: HasTwoFactorAuth,
                isVerified: IsVerified,
                email: Email,
                isWebhookUser: IsWebhookUser);
        }
    }
}
