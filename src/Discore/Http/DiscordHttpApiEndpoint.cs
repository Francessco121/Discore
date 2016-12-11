using Discore.Http.Net;
using System;

namespace Discore.Http
{
    public abstract class DiscordHttpApiEndpoint
    {
        internal IDiscordApplication App { get; }
        internal RestClient Rest { get; }

        internal DiscordHttpApiEndpoint(IDiscordApplication app, RestClient rest)
        {
            App = app;
            Rest = rest;
        }

        protected DiscordChannel GetChannelAsProperChannel(DiscordApiData data)
        {
            bool isPrivate = data.GetBoolean("is_private") ?? false;

            if (isPrivate) // if dm channel
                return new DiscordDMChannel(App, data);
            else
            {
                string channelType = data.GetString("type");

                if (channelType == "voice") // if voice channel
                    return new DiscordGuildVoiceChannel(App, data);
                else if (channelType == "text") // if text channel
                    return new DiscordGuildTextChannel(App, data);
            }

            throw new NotSupportedException($"{nameof(Snowflake)} isn't a known type of {nameof(DiscordChannel)}.");
        }
    }
}
