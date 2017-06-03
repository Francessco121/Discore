using Discore.Http.Net;
using System;

namespace Discore.Http
{
    public sealed partial class DiscordHttpApi : IDisposable
    {
        /// <summary>
        /// Gets or sets whether to resend requests that get rate-limited.
        /// </summary>
        public bool RetryWhenRateLimited
        {
            get => rest.RetryOnRateLimit;
            set => rest.RetryOnRateLimit = value;
        }

        RestClient rest;
        IDiscordApplication app;

        internal DiscordHttpApi(IDiscordApplication app, InitialHttpApiSettings settings)
        {
            this.app = app;
            rest = new RestClient(app.Authenticator, settings);
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
