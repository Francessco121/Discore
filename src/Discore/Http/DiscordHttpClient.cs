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
            bool isPrivate = data.GetBoolean("is_private") ?? false;

            if (isPrivate) // if dm channel
                return new DiscordDMChannel(this, data);
            else
            {
                string channelType = data.GetString("type");

                if (channelType == "voice") // if voice channel
                    return new DiscordGuildVoiceChannel(this, data);
                else if (channelType == "text") // if text channel
                    return new DiscordGuildTextChannel(this, data);
            }

            throw new NotSupportedException($"{nameof(Snowflake)} isn't a known type of {nameof(DiscordChannel)}.");
        }

        public void Dispose()
        {
            rest.Dispose();
        }
    }
}
