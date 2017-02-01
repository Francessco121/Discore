using Discore.Http.Net;
using Discore.Voice;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    public sealed class DiscordHttpVoiceEndpoint : DiscordHttpApiEndpoint
    {
        internal DiscordHttpVoiceEndpoint(IDiscordApplication app, RestClient rest) 
            : base(app, rest)
        { }

        /// <summary>
        /// Gets a list of available voice regions.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordVoiceRegion>> GetVoiceRegions()
        {
            DiscordApiData data = await Rest.Get("/voice/regions", "ListVoiceRegions").ConfigureAwait(false);

            DiscordVoiceRegion[] regions = new DiscordVoiceRegion[data.Values.Count];
            for (int i = 0; i < regions.Length; i++)
                regions[i] = new DiscordVoiceRegion(data.Values[i]);

            return regions;
        }
    }
}
