using Discore.Http.Net;

namespace Discore.Http
{
    public sealed class DiscordHttpApi
    {
        public DiscordHttpUsersEndpoint Users { get; }
        public DiscordHttpChannelsEndpoint Channels { get; }
        public DiscordHttpWebhookEndpoint Webhooks { get; }
        public DiscordHttpVoiceEndpoint Voice { get; }

        internal HttpApi InternalApi { get; }

        internal DiscordHttpApi(IDiscordApplication app, HttpApi api)
        {
            InternalApi = api;

            Users = new DiscordHttpUsersEndpoint(app, api.Users);
            Channels = new DiscordHttpChannelsEndpoint(api.Channels);
            Webhooks = new DiscordHttpWebhookEndpoint(api.Webhooks);
            Voice = new DiscordHttpVoiceEndpoint(api.Voice);
        }
    }
}
