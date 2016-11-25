using Discore.Http.Net;

namespace Discore.Http
{
    public sealed class DiscordHttpApi
    {
        public DiscordHttpUsersEndpoint Users { get; }
        public DiscordHttpWebhookEndpoint Webhooks { get; }

        internal DiscordHttpApi(HttpApi api)
        {
            Users = new DiscordHttpUsersEndpoint(api.Users);
            Webhooks = new DiscordHttpWebhookEndpoint(api.Webhooks);
        }
    }
}
