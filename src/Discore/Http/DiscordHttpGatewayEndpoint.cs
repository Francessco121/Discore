using Discore.Http.Net;
using System.Threading;
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

    public class DiscordHttpGatewayEndpoint : DiscordHttpApiEndpoint
    {
        internal DiscordHttpGatewayEndpoint(IDiscordApplication app, RestClient rest) 
            : base(app, rest)
        { }

        /// <summary>
        /// Gets the minimum number of required shards for the current authenticated Discord application.
        /// </summary>
        public async Task<int> GetBotRequiredShards()
        {
            GatewayBotResponse response = await GetBot();
            return response.Shards;
        }

        internal async Task<string> Get(CancellationToken cancellationToken)
        {
            DiscordApiData data = await Rest.Get("gateway", "GetGateway", cancellationToken);
            return data.GetString("url");
        }

        internal async Task<GatewayBotResponse> GetBot()
        {
            DiscordApiData data = await Rest.Get("gateway/bot", "GetGatewayBot");
            return new GatewayBotResponse(data);
        }
    }
}
