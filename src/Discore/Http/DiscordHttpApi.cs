using Discore.Http.Net;

namespace Discore.Http
{
    public sealed class DiscordHttpApi
    {
        public DiscordHttpUsersEndpoint Users { get; }
        public DiscordHttpWebhookEndpoint Webhooks { get; }

        internal HttpApi InternalApi { get; }

        internal DiscordHttpApi(IDiscordApplication app, HttpApi api)
        {
            InternalApi = api;

            Users = new DiscordHttpUsersEndpoint(app, api.Users);
            Webhooks = new DiscordHttpWebhookEndpoint(api.Webhooks);
        }
    }
}
