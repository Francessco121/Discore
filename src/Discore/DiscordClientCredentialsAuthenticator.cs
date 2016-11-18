using System;
using System.Net.Http;

namespace Discore
{
    public class DiscordClientCredentialsAuthenticator : IDiscordAuthenticator
    {
        public bool CanAuthenticateWebSocket { get { return false; } }

        string token;
        string tokenType;

        public DiscordClientCredentialsAuthenticator(string clientId, string clientSecret)
        {
            throw new NotImplementedException("This authenticator is just a stub for now."); // This is not useable yet!

            using (HttpClient http = new HttpClient())
            {
                string result;

                try
                {
                    result = http.GetStringAsync("https://discordapp.com/api/oauth2/token").Result;
                }
                catch (AggregateException aex)
                {
                    throw aex.InnerException;
                }

                DiscordApiData data = DiscordApiData.ParseJson(result);
                token = data.GetString("access_token");
                tokenType = data.GetString("token_type");

                // TODO: this method requires some kind of re-authentication!
            }
        }

        public string GetToken()
        {
            return token;
        }

        public string GetTokenHttpType()
        {
            return tokenType;
        }
    }
}
