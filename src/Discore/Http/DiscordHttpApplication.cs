using Discore.Http.Net;

namespace Discore.Http
{
    /// <summary>
    /// A Discord application which only works with the Discord http/restful api.
    /// </summary>
    public class DiscordHttpApplication : IDiscordApplication
    {
        /// <summary>
        /// Gets the authenticator used for this application.
        /// </summary>
        public IDiscordAuthenticator Authenticator { get; }
        /// <summary>
        /// Gets an interface for the Discord http/restful api.
        /// </summary>
        public DiscordHttpApi HttpApi { get; }

        public DiscordHttpApplication(IDiscordAuthenticator authenticator)
        {
            Authenticator = authenticator;

            HttpApi api = new HttpApi(authenticator);
            HttpApi = new DiscordHttpApi(this, api);
        }
    }
}
