using Discore.Http;

namespace Discore.WebSocket
{
    class MutableUser : MutableEntity<DiscordUser>
    {
        public Snowflake Id { get; }

        public bool IsWebhookUser { get; private set; }
        public string Username { get; private set; }
        public string Discriminator { get; private set; }
        public string Avatar { get; private set; }
        public bool IsBot { get; private set; }
        public bool HasTwoFactorAuth { get; private set; }
        public bool IsVerified { get; private set; }
        public string Email { get; private set; }

        public MutableUser(Snowflake id, DiscordHttpApi http)
            : base(http)
        {
            Id = id;
        }

        public void Update(DiscordApiData data)
        {
            IsWebhookUser = data.ContainsKey("webhook_id");

            Username = data.GetString("username");
            Discriminator = data.GetString("discriminator");
            Avatar = data.GetString("avatar");
            IsBot = data.GetBoolean("bot") ?? false;
            HasTwoFactorAuth = data.GetBoolean("mfa_enabled") ?? false;
            IsVerified = data.GetBoolean("verified") ?? false;
            Email = data.GetString("email");

            Dirty();
        }

        protected override DiscordUser BuildImmutableEntity()
        {
            return new DiscordUser(this);
        }
    }
}
