using Discore.Http;

namespace Discore
{
    public interface IDiscordApplication
    {
        /// <summary>
        /// Gets the bot user token for this application.
        /// </summary>
        string BotToken { get; }
        /// <summary>
        /// Gets an interface for the Discord http/restful api.
        /// </summary>
        DiscordHttpClient HttpApi { get; }
    }
}
