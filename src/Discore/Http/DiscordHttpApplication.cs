using System;

namespace Discore.Http
{
    /// <summary>
    /// A Discord application which only works with the Discord http/restful api.
    /// </summary>
    public class DiscordHttpApplication : IDiscordApplication, IDisposable
    {
        /// <summary>
        /// Gets the bot user token for this application.
        /// </summary>
        public string BotToken { get; }
        /// <summary>
        /// Gets an interface for the Discord http/restful api.
        /// </summary>
        public DiscordHttpClient HttpApi { get; }

        public DiscordHttpApplication(string botToken)
        {
            BotToken = botToken;
            HttpApi = new DiscordHttpClient(botToken);
        }

        public void Dispose()
        {
            HttpApi.Dispose();
        }
    }
}
