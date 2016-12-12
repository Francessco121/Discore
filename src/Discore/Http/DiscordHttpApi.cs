using Discore.Http.Net;

namespace Discore.Http
{
    public sealed class DiscordHttpApi
    {
        public DiscordHttpUserEndpoint Users { get; }
        public DiscordHttpChannelEndpoint Channels { get; }
        public DiscordHttpWebhookEndpoint Webhooks { get; }
        public DiscordHttpVoiceEndpoint Voice { get; }
        public DiscordHttpGuildEndpoint Guilds { get; }
        public DiscordHttpInviteEndpoint Invites { get; }

        internal DiscordHttpGatewayEndpoint Gateway { get; }

        internal DiscordHttpApi(IDiscordApplication app)
        {
            RestClient rest = new RestClient(app.Authenticator);

            Gateway = new DiscordHttpGatewayEndpoint(app, rest);

            Users = new DiscordHttpUserEndpoint(app, rest);
            Channels = new DiscordHttpChannelEndpoint(app, rest);
            Webhooks = new DiscordHttpWebhookEndpoint(app, rest);
            Voice = new DiscordHttpVoiceEndpoint(app, rest);
            Guilds = new DiscordHttpGuildEndpoint(app, rest);
            Invites = new DiscordHttpInviteEndpoint(app, rest);
        }
    }
}
