using Discore.Http;
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

        /// <param name="authenticator">The method of authentication used by the application.</param>
        /// <param name="httpApiSettings">The initial settings for the HTTP API. Uses the default settings if left null.</param>
        /// <exception cref="ArgumentException">Thrown if the passed authenticator does not support WebSockets.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        public DiscordWebSocketApplication(IDiscordAuthenticator authenticator, InitialHttpApiSettings httpApiSettings = null)
        {
            if (!authenticator.CanAuthenticateWebSocket)
                throw new ArgumentException("Authentication must support WebSockets.", nameof(authenticator));

            Authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));

            ShardManager = new ShardManager(this);
            HttpApi = new DiscordHttpApi(this, httpApiSettings ?? new InitialHttpApiSettings());
        }

        public void Dispose()
        {
            ShardManager.Dispose();
            HttpApi.Dispose();
        }
    }
}
