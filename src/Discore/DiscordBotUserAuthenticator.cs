namespace Discore
{
    public class DiscordBotUserAuthenticator : IDiscordAuthenticator
    {
        public bool CanAuthenticateWebSocket { get { return true; } }

        string token;

        public DiscordBotUserAuthenticator(string token)
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
