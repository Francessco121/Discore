using Discore.Http.Net;
using System;

namespace Discore.Http
{
    public sealed class DiscordHttpApi : IDisposable
    {
        /// <summary>
        /// Gets the implementation of the /users section of the HTTP API.
        /// </summary>
        public DiscordHttpUserEndpoint Users { get; }
        /// <summary>
        /// Gets the implementation of the /channels section of the HTTP API.
        /// </summary>
        public DiscordHttpChannelEndpoint Channels { get; }
        /// <summary>
        /// Gets the implementation of the /webhooks section of the HTTP API.
        /// </summary>
        public DiscordHttpWebhookEndpoint Webhooks { get; }
        /// <summary>
        /// Gets the implementation of the /voice section of the HTTP API.
        /// </summary>
        public DiscordHttpVoiceEndpoint Voice { get; }
        /// <summary>
        /// Gets the implementation of the /guilds section of the HTTP API.
        /// </summary>
        public DiscordHttpGuildEndpoint Guilds { get; }
        /// <summary>
        /// Gets the implementation of the /invites section of the HTTP API.
        /// </summary>
        public DiscordHttpInviteEndpoint Invites { get; }
        /// <summary>
        /// Gets the implementation of the /gateway section of the HTTP API.
        /// </summary>
        public DiscordHttpGatewayEndpoint Gateway { get; }

        /// <summary>
        /// Gets or sets whether to resend requests that get rate-limited.
        /// </summary>
        public bool RetryWhenRateLimited
        {
            get { return rest.RetryOnRateLimit; }
            set { rest.RetryOnRateLimit = value; }
        }

        RestClient rest;

        internal DiscordHttpApi(IDiscordApplication app, InitialHttpApiSettings settings)
        {
            rest = new RestClient(app.Authenticator, settings);

            Gateway = new DiscordHttpGatewayEndpoint(app, rest);
            Users = new DiscordHttpUserEndpoint(app, rest);
            Channels = new DiscordHttpChannelEndpoint(app, rest);
            Webhooks = new DiscordHttpWebhookEndpoint(app, rest);
            Voice = new DiscordHttpVoiceEndpoint(app, rest);
            Guilds = new DiscordHttpGuildEndpoint(app, rest);
            Invites = new DiscordHttpInviteEndpoint(app, rest);
        }

        public void Dispose()
        {
            rest.Dispose();
        }
    }
}
