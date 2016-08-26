using System.Threading.Tasks;

namespace Discore.Net
{
    class RestClientInternalService : RestClientService
    {
        public RestClientInternalService(DiscordClient client, RestClient rest) 
            : base(client, rest)
        { }

        public async Task<string> GetGatewayEndpoint()
        {
            DiscordApiData data = await Get("gateway", "GetGatewayEndpoint");
            return data.GetString("url");
        }
    }
}
