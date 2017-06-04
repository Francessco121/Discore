using System.Threading.Tasks;

namespace Discore.Http
{
    class GatewayBotResponse
    {
        public string Url { get; }
        public int Shards { get; }

        public GatewayBotResponse(DiscordApiData data)
        {
            Url = data.GetString("url");
            Shards = data.GetInteger("shards").Value;
        }
    }

    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets the minimum number of required shards for the current authenticated Discord application.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<int> GetBotRequiredShards()
        {
            GatewayBotResponse response = await GetBot().ConfigureAwait(false);
            return response.Shards;
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        internal async Task<string> Get()
        {
            DiscordApiData data = await rest.Get("gateway",
                "gateway").ConfigureAwait(false);
            return data.GetString("url");
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        internal async Task<GatewayBotResponse> GetBot()
        {
            DiscordApiData data = await rest.Get("gateway/bot",
                "gateway/bot").ConfigureAwait(false);
            return new GatewayBotResponse(data);
        }
    }
}
