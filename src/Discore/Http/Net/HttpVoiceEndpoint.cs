using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class HttpVoiceEndpoint : HttpApiEndpoint
    {
        public HttpVoiceEndpoint(RestClient restClient) 
            : base(restClient)
        { }

        public Task<DiscordApiData> ListVoiceRegions()
        {
            return Rest.Get("/voice/regions", "ListVoiceRegions");
        }
    }
}
