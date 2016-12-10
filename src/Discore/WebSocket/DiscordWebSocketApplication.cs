using Discore.Http;
using Discore.Http.Net;
using System;

namespace Discore.WebSocket
{
    /// <summary>
    /// A Discord bot application.
    /// </summary>
    public class DiscordWebSocketApplication : IDiscordApplication, IDisposable
    {
        /// <summary>
        /// Gets the manager of each shard used by this process.
        /// </summary>
        public ShardManager ShardManager { get; }
        /// <summary>
        /// Gets the authenticator used for this application.
        /// </summary>
        public IDiscordAuthenticator Authenticator { get; }
        /// <summary>
        /// Gets an interface for the Discord http/restful api.
        /// </summary>
        public DiscordHttpApi HttpApi { get; }

        public DiscordWebSocketApplication(IDiscordAuthenticator authenticator)
        {
            if (!authenticator.CanAuthenticateWebSocket)
                throw new ArgumentException("Authentication must support websockets.", "authenticator");

            Authenticator = authenticator;

            ShardManager = new ShardManager(this);
            HttpApi = new DiscordHttpApi(this);
        }

        public void Dispose()
        {
            ShardManager.Dispose();
        }
    }
}
