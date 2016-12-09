using Discore.Http.Net;
using System;
using System.Threading.Tasks;

namespace Discore
{
    public sealed class DiscordGuildVoiceChannel : DiscordGuildChannel
    {
        /// <summary>
        /// Gets the audio bitrate used for this channel.
        /// </summary>
        public int Bitrate { get; }

        /// <summary>
        /// Gets the maximum number of users that can be connected to this channel simultaneously.
        /// </summary>
        public int UserLimit { get; }

        HttpChannelsEndpoint channelsHttp;

        internal DiscordGuildVoiceChannel(IDiscordApplication app, DiscordApiData data, Snowflake? guildId = null)
            : base(app, data, DiscordGuildChannelType.Voice, guildId)
        {
            channelsHttp = app.HttpApi.InternalApi.Channels;

            Bitrate = data.GetInteger("bitrate").Value;
            UserLimit = data.GetInteger("user_limit").Value;
        }

        /// <summary>
        /// Modifies this voice channel.
        /// Any parameters not specified will be unchanged.
        /// </summary>
        public void Modify(string name = null, int? position = null, int? bitrate = null, int? userLimit = null)
        {
            try { ModifyAsync(name, position, bitrate, userLimit).Wait(); }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Modifies this voice channel.
        /// Any parameters not specified will be unchanged.
        /// </summary>
        public async Task ModifyAsync(string name = null, int? position = null, int? bitrate = null, int? userLimit = null)
        {
            await channelsHttp.Modify(Id, name, position, null, bitrate, userLimit);
        }
    }
}
