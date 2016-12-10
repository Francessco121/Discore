using Discore.Http.Net;
using Discore.Voice;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    public sealed class DiscordHttpVoiceEndpoint
    {
        RestClient Rest;

        internal DiscordHttpVoiceEndpoint(RestClient restClient)
        {
            Rest = restClient;
        }

        public async Task<IReadOnlyList<DiscordVoiceRegion>> GetVoiceReaions()
        {
            DiscordApiData data = await Rest.Get("/voice/regions", "ListVoiceRegions");

            List<DiscordVoiceRegion> toReturn = new List<DiscordVoiceRegion>();
            foreach (DiscordApiData item in data.Values)
                toReturn.Add(new DiscordVoiceRegion(item));

            return toReturn;
        }
    }
}
