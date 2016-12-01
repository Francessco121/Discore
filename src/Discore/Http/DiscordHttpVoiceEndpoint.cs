using Discore.Http.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    public sealed class DiscordHttpVoiceEndpoint
    {
        HttpVoiceEndpoint endpoint;

        internal DiscordHttpVoiceEndpoint(HttpVoiceEndpoint endpoint)
        {
            this.endpoint = endpoint;
        }

        public async Task<IReadOnlyList<DiscordVoiceRegion>> GetVoiceReaions()
        {
            DiscordApiData data = await endpoint.ListVoiceRegions();
            List<DiscordVoiceRegion> toReturn = new List<DiscordVoiceRegion>();
            foreach (DiscordApiData item in data.Values)
                toReturn.Add(new DiscordVoiceRegion(item));

            return toReturn;
        }
    }
}
