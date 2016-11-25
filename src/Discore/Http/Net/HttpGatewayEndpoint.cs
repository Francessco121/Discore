using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class HttpGatewayEndpoint : HttpApiEndpoint
    {
        public HttpGatewayEndpoint(RestClient restClient) 
            : base(restClient)
        { }

        public async Task<DiscordApiData> Get()
        {
            return await Rest.Get("gateway", "GetGateway");
        }

        public async Task<DiscordApiData> GetBot()
        {
            return await Rest.Get("gateway/bot", "GetGatewayBot");
        }
    }
}
