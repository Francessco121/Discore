namespace Discore
{
    public class DiscordBotUserToken : IDiscordAuthenticator
    {
        public bool CanAuthenticateWebSocket { get { return true; } }

        string token;

        public DiscordBotUserToken(string token)
        {
            this.token = token;
        }

        public string GetToken()
        {
            return token;
        }

        public string GetTokenHttpType()
        {
            return "Bot";
        }
    }
}
