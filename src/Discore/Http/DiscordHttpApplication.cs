using System;

namespace Discore.Http
{
    /// <summary>
    /// A Discord application which only works with the Discord http/restful api.
    /// </summary>
    public class DiscordHttpApplication : IDiscordApplication, IDisposable
    {
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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="authenticator"/> is null.</exception>
        public DiscordHttpApplication(IDiscordAuthenticator authenticator, InitialHttpApiSettings httpApiSettings = null)
        {
            Authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));

            HttpApi = new DiscordHttpApi(this, httpApiSettings ?? new InitialHttpApiSettings());
        }

        public void Dispose()
        {
            HttpApi.Dispose();
        }
    }
}
