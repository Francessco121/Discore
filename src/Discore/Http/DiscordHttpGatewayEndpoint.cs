using Discore.Http.Net;
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

    class DiscordHttpGatewayEndpoint : DiscordHttpApiEndpoint
    {
        internal DiscordHttpGatewayEndpoint(IDiscordApplication app, RestClient rest) 
            : base(app, rest)
        { }

        public async Task<string> Get()
        {
            DiscordApiData data = await Rest.Get("gateway", "GetGateway");
            return data.GetString("url");
        }

        public async Task<GatewayBotResponse> GetBot()
        {
            DiscordApiData data = await Rest.Get("gateway/bot", "GetGatewayBot");
            return new GatewayBotResponse(data);
        }
    }
}
