using Discore.Http.Net;

namespace Discore.Http
{
    public sealed class DiscordHttpApi
    {
        public DiscordHttpUsersEndpoint Users { get; }
        public DiscordHttpChannelsEndpoint Channels { get; }
        public DiscordHttpWebhookEndpoint Webhooks { get; }

        internal DiscordHttpApi(HttpApi api)
        {
            Users = new DiscordHttpUsersEndpoint(api.Users);
            Channels = new DiscordHttpChannelsEndpoint(api.Channels);
            Webhooks = new DiscordHttpWebhookEndpoint(api.Webhooks);
        }
    }
}
