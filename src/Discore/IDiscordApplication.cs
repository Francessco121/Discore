using Discore.Http;

namespace Discore
{
    public interface IDiscordApplication
    {
        /// <summary>
        /// Gets the authenticator used for this application.
        /// </summary>
        IDiscordAuthenticator Authenticator { get; }
        /// <summary>
        /// Gets an interface for the Discord http/restful api.
        /// </summary>
        DiscordHttpApi HttpApi { get; }
    }
}
