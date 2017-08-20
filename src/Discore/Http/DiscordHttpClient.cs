using Discore.Http.Net;
using System;

namespace Discore.Http
{
    public sealed partial class DiscordHttpClient : IDisposable
    {
        /// <summary> 
        /// Gets or sets whether a single HTTP client should be used for all API requests per 
        /// <see cref="DiscordHttpClient"/> instance. 
        /// <para>In rare cases using a single client causes requests to hang until they timeout 
        /// (believed to be a .NET Core bug).</para>
        /// <para>This is true by default.</para> 
        /// <para>Note: This only applies to newly created <see cref="DiscordHttpClient"/> instances.</para> 
        /// </summary> 
        public static bool UseSingleHttpClient { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to resend requests that get rate-limited.
        /// </summary>
        public bool RetryWhenRateLimited
        {
            get => rest.RetryOnRateLimit;
            set => rest.RetryOnRateLimit = value;
        }

        RestClient rest;

        public DiscordHttpClient(string botToken)
        {
            rest = new RestClient(botToken);
        }

        DiscordChannel DeserializeChannelData(DiscordApiData data)
        {
            DiscordChannelType type = (DiscordChannelType)data.GetInteger("type").Value;

            if (type == DiscordChannelType.DirectMessage)
                return new DiscordDMChannel(this, data);
            else if (type == DiscordChannelType.GuildText)
                return new DiscordGuildTextChannel(this, data);
            else if (type == DiscordChannelType.GuildVoice)
                return new DiscordGuildVoiceChannel(this, data);
            else
                throw new NotSupportedException($"{nameof(Snowflake)} isn't a known type of {nameof(DiscordChannel)}.");
        }

        public void Dispose()
        {
            rest.Dispose();
        }
    }
}
