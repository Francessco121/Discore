using Discore.Http.Net;

namespace Discore.Http
{
    public sealed class DiscordHttpApi
    {
        public DiscordHttpUsersEndpoint Users { get; }
        public DiscordHttpChannelsEndpoint Channels { get; }
        public DiscordHttpWebhookEndpoint Webhooks { get; }
        public DiscordHttpVoiceEndpoint Voice { get; }
        public DiscordHttpGuildsEndpoint Guilds { get; }

        internal DiscordHttpGatewayEndpoint Gateway { get; }

        internal DiscordHttpApi(IDiscordApplication app)
        {
            RestClient rest = new RestClient(app.Authenticator);

            Gateway = new DiscordHttpGatewayEndpoint(app, rest);

            Users = new DiscordHttpUsersEndpoint(app, rest);
            Channels = new DiscordHttpChannelsEndpoint(app, rest);
            Webhooks = new DiscordHttpWebhookEndpoint(app, rest);
            Voice = new DiscordHttpVoiceEndpoint(app, rest);
            Guilds = new DiscordHttpGuildsEndpoint(app, rest);
        }
    }
}
