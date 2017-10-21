using Discore.Http;
using System;
using System.Threading.Tasks;

namespace Discore
{
    public sealed class DiscordGuildCategoryChannel : DiscordGuildChannel
    {
        readonly DiscordHttpClient http;

        internal DiscordGuildCategoryChannel(DiscordHttpClient http, DiscordApiData data, 
            Snowflake? guildId = null)
            : base(http, data, DiscordChannelType.GuildCategory, guildId)
        {
            this.http = http;
        }

        /// <summary>
        /// Modifies this category channel's settings.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="options">A set of options to modify the channel with</param>
        /// <returns>Returns the updated category channel.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildCategoryChannel> Modify(GuildCategoryChannelOptions options)
        {
            return http.ModifyCategoryChannel(Id, options);
        }
    }
}
