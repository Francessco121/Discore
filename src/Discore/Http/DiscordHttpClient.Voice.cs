using Discore.Voice;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

#nullable enable

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
            using JsonDocument? data = await rest.Get("voice/regions", "voice/regions").ConfigureAwait(false);

            JsonElement values = data!.RootElement;

            var regions = new DiscordVoiceRegion[values.GetArrayLength()];
            for (int i = 0; i < regions.Length; i++)
                regions[i] = new DiscordVoiceRegion(values[i]);

            return regions;
        }

        /// <summary>
        /// Gets a list of all voice regions available to the specified guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegions(Snowflake guildId)
        {
            using JsonDocument? data = await rest.Get($"guilds/{guildId}/regions",
                $"guilds/{guildId}/regions").ConfigureAwait(false);

            JsonElement values = data!.RootElement;

            var regions = new DiscordVoiceRegion[values.GetArrayLength()];
            for (int i = 0; i < regions.Length; i++)
                regions[i] = new DiscordVoiceRegion(values[i]);

            return regions;
        }

        /// <summary>
        /// Gets a list of all voice regions available to the specified guild.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegions(DiscordGuild guild)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return GetGuildVoiceRegions(guild.Id);
        }
    }
}

#nullable restore
