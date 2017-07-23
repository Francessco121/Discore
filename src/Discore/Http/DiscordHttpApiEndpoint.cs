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

        internal DiscordChannel DeserializeChannelData(DiscordApiData data)
        {
            InternalChannelType type = (InternalChannelType)data.GetInteger("type").Value;

            if (type == InternalChannelType.DM)
                return new DiscordDMChannel(App, data);
            else if (type == InternalChannelType.GuildText)
                return new DiscordGuildTextChannel(App, data);
            else if (type == InternalChannelType.GuildVoice)
                return new DiscordGuildVoiceChannel(App, data);
            else
                throw new NotSupportedException($"{nameof(Snowflake)} isn't a known type of {nameof(DiscordChannel)}.");
        }
    }
}
