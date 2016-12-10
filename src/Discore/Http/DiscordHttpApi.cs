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

            RestClient rest = new RestClient(app.Authenticator);

            Users = new DiscordHttpUsersEndpoint(app, rest);
            Channels = new DiscordHttpChannelsEndpoint(app, rest);
            Webhooks = new DiscordHttpWebhookEndpoint(api.Webhooks);
            Voice = new DiscordHttpVoiceEndpoint(api.Voice);
        }
    }
}
