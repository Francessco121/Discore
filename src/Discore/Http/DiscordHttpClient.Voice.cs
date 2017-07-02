using Discore.Voice;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets a list of available voice regions.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordVoiceRegion>> ListVoiceRegions()
        {
            DiscordApiData data = await rest.Get("voice/regions", "voice/regions").ConfigureAwait(false);

            DiscordVoiceRegion[] regions = new DiscordVoiceRegion[data.Values.Count];
            for (int i = 0; i < regions.Length; i++)
                regions[i] = new DiscordVoiceRegion(data.Values[i]);

            return regions;
        }

        /// <summary>
        /// Gets a list of all voice regions available to the specified guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegions(Snowflake guildId)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/regions",
                $"guilds/{guildId}/regions").ConfigureAwait(false);

            DiscordVoiceRegion[] regions = new DiscordVoiceRegion[data.Values.Count];
            for (int i = 0; i < regions.Length; i++)
                regions[i] = new DiscordVoiceRegion(data.Values[i]);

            return regions;
        }
    }
}
