namespace Discore
{
    public class DiscordBotUserToken : IDiscordAuthenticator
    {
        /// <summary>
        /// Will always return true, as bot user tokens can be used to authenticate with the WebSocket API.
        /// </summary>
        public bool CanAuthenticateWebSocket => true;

        string token;

        public DiscordBotUserToken(string token)
        {
            this.token = token;
        }

        /// <summary>
        /// Gets the bot user token this object represents.
        /// </summary>
        public string GetToken()
        {
            return token;
        }

        /// <summary>
        /// Returns "Bot".
        /// </summary>
        public string GetTokenHttpType()
        {
            return "Bot";
        }
    }
}
