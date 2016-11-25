//using System;
//using System.Collections.Generic;
//using System.Net.Http;

//namespace Discore
//{
//    public class DiscordClientCredentials : IDiscordAuthenticator
//    {
//        public bool CanAuthenticateWebSocket { get { return false; } }

//        string token;
//        string tokenType;

//        public DiscordClientCredentials(string clientId, string clientSecret)
//        {
//            throw new NotImplementedException("This authenticator is just a stub for now."); // This is not useable yet!

//            using (HttpClient http = new HttpClient())
//            {
//                string result;

//                try
//                {
//                    Dictionary<string, string> props = new Dictionary<string, string>();
//                    FormUrlEncodedContent content = new FormUrlEncodedContent(props);
//                    props["client_id"] = clientId;
//                    props["client_secret"] = clientSecret;
//                    props["grant_type"] = "client_credentials";

//                    HttpResponseMessage response = http.PostAsync("https://discordapp.com/api/oauth2/token", content).Result;
//                    result = response.Content.ReadAsStringAsync().Result;
//                }
//                catch (AggregateException aex)
//                {
//                    throw aex.InnerException;
//                }

//                DiscordApiData data = DiscordApiData.ParseJson(result);
//                token = data.GetString("access_token");
//                tokenType = data.GetString("token_type");

//                // TODO: this method requires some kind of re-authentication!
//            }
//        }

//        public string GetToken()
//        {
//            return token;
//        }

//        public string GetTokenHttpType()
//        {
//            return tokenType;
//        }
//    }
//}
