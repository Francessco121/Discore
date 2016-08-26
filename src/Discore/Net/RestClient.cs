using System.Net.Http;

namespace Discore.Net
{
    class RestClient : IDiscordRestClient
    {
        public const string BASE_URL = "https://discordapp.com/api";

        public IDiscordRestMessagesService Messages { get; }
        public IDiscordRestChannelsService Channels { get; }

        public RestClientRateLimitManager RateLimitManager { get; }
        public HttpClient HttpClient { get; }
        public RestClientInternalService Internal { get; }

        DiscordClient client;
        DiscordApiCacheHelper cacheHelper;

        public RestClient(DiscordClient client)
        {
            this.client = client;
            cacheHelper = client.CacheHelper;

            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "DiscordBot (discordio_sharp, 1.0)");

            RateLimitManager = new RestClientRateLimitManager();
            Internal = new RestClientInternalService(client, this);

            Messages = new RestClientMessagesService(client, this);
            Channels = new RestClientChannelsService(client, this);
        }

        public void SetToken(string token)
        {
            HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bot {token}");
        }
    }
}
