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
        /// Gets the bot user token for this application.
        /// </summary>
        public string BotToken { get; }
        /// <summary>
        /// Gets an interface for the Discord http/restful api.
        /// </summary>
        public DiscordHttpClient HttpApi { get; }

        public DiscordWebSocketApplication(string botToken)
        {
            BotToken = botToken;

            ShardManager = new ShardManager(this);
            HttpApi = new DiscordHttpClient(botToken);
        }

        public void Dispose()
        {
            ShardManager.Dispose();
            HttpApi.Dispose();
        }
    }
}
