using Discore.Http;
using Discore.Http.Net;
using Discore.WebSocket;
using System;

namespace Discore
{
    /// <summary>
    /// A Discord bot application.
    /// </summary>
    public class DiscordWebSocketApplication : IDisposable
    {
        /// <summary>
        /// Gets the authenticator used for this application.
        /// </summary>
        public IDiscordAuthenticator Authenticator { get; }
        /// <summary>
        /// Gets the manager of each shard used by this process.
        /// </summary>
        public ShardManager ShardManager { get; }
        /// <summary>
        /// Gets an interface for the Discord http/restful api.
        /// </summary>
        public DiscordHttpApi HttpApi { get; }

        internal HttpApi InternalHttpApi { get; }

        public DiscordWebSocketApplication(IDiscordAuthenticator authenticator)
        {
            if (!authenticator.CanAuthenticateWebSocket)
                throw new ArgumentException("Authentication must support websockets.", "authenticator");

            Authenticator = authenticator;

            ShardManager = new ShardManager(this);
            InternalHttpApi = new HttpApi(authenticator);
            HttpApi = new DiscordHttpApi(InternalHttpApi);
        }

        public void Dispose()
        {
            ShardManager.Dispose();
        }
    }
}
