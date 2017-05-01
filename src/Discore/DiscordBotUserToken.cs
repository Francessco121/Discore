using System;

namespace Discore
{
    public class DiscordBotUserToken : IDiscordAuthenticator
    {
        /// <summary>
        /// Will always return true, as bot user tokens can be used to authenticate with the WebSocket API.
        /// </summary>
        public bool CanAuthenticateWebSocket => true;

        string token;

        /// <exception cref="ArgumentException">Thrown if the token is empty or only contains whitespace cahracters.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        public DiscordBotUserToken(string token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty or only contain whitespace characters.", nameof(token));

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
