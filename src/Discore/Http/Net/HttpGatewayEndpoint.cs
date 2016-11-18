namespace Discore.Http.Net
{
    class HttpGatewayEndpoint : HttpApiEndpoint
    {
        public HttpGatewayEndpoint(RestClient restClient) 
            : base(restClient)
        { }

        public DiscordApiData Get()
        {
            return Rest.Get("gateway", "GetGateway");
        }

        public DiscordApiData GetBot()
        {
            return Rest.Get("gateway/bot", "GetGatewayBot");
        }
    }
}
