using Discore.Http;
using System;
using System.Threading.Tasks;

namespace Discore
{
    public sealed class DiscordGuildStoreChannel : DiscordGuildChannel
    {
        /// <summary>
        /// Gets whether this store channel is NSFW (not-safe-for-work).
        /// </summary>
        public bool Nsfw { get; }

        /// <summary>
        /// Gets the ID of the parent category channel or null if the channel is not in a category.
        /// </summary>
        public Snowflake? ParentId { get; }

        readonly DiscordHttpClient http;

        internal DiscordGuildStoreChannel(DiscordHttpClient http, DiscordApiData data, Snowflake? guildId = null) 
            : base(http, data, DiscordChannelType.GuildStore, guildId)
        {
            this.http = http;

            Nsfw = data.GetBoolean("nsfw") ?? false;
            ParentId = data.GetSnowflake("parent_id");
        }

        /// <summary>
        /// Modifies this store channel's settings.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="options">A set of options to modify the channel with</param>
        /// <returns>Returns the updated store channel.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildStoreChannel> Modify(GuildStoreChannelOptions options)
        {
            return http.ModifyStoreChannel(Id, options);
        }
    }
}
