using System.Text.Json;
using System.Threading.Tasks;

namespace Discore.Http
{
    class GatewayBotResponse
    {
        public string Url { get; }
        public int Shards { get; }

        internal GatewayBotResponse(JsonElement json)
        {
            Url = json.GetProperty("url").GetString()!;
            Shards = json.GetProperty("shards").GetInt32();
        }
    }

    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets the minimum number of required shards for the current bot.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<int> GetBotRequiredShards()
        {
            GatewayBotResponse response = await GetGatewayBot().ConfigureAwait(false);
            return response.Shards;
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        internal async Task<string> GetGateway()
        {
            using JsonDocument? data = await api.Get("gateway",
                "gateway").ConfigureAwait(false);

            return data!.RootElement.GetProperty("url").GetString()!;
        }

        /// <exception cref="DiscordHttpApiException"></exception>
        internal async Task<GatewayBotResponse> GetGatewayBot()
        {
            using JsonDocument? data = await api.Get("gateway/bot",
                "gateway/bot").ConfigureAwait(false);

            return new GatewayBotResponse(data!.RootElement);
        }
    }
}
