namespace Discore.Net.Rest
{
    public class DiscordRestGatewayEndpoint : DiscordRestEndpoint
    {
        internal DiscordRestGatewayEndpoint(RestClient restClient) 
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
