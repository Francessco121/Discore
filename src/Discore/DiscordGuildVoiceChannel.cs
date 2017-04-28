using Discore.Http;
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

        DiscordHttpChannelEndpoint channelsHttp;

        internal DiscordGuildVoiceChannel(IDiscordApplication app, DiscordApiData data, Snowflake? guildId = null)
            : base(app, data, DiscordGuildChannelType.Voice, guildId)
        {
            channelsHttp = app.HttpApi.Channels;

            Bitrate = data.GetInteger("bitrate").Value;
            UserLimit = data.GetInteger("user_limit").Value;
        }

        #region Deprecated Modify
        /// <summary>
        /// Modifies this voice channel.
        /// Any parameters not specified will be unchanged.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        [Obsolete("Please use the Modify overload with a builder object instead.")]
        public Task<DiscordGuildVoiceChannel> Modify(string name = null, int? position = null, 
            int? bitrate = null, int? userLimit = null)
        {
            return channelsHttp.Modify<DiscordGuildVoiceChannel>(Id, name, position, null, bitrate, userLimit);
        }
        #endregion

        /// <summary>
        /// Modifies this voice channel's settings.
        /// </summary>
        /// <param name="parameters">A set of parameters to modify the channel with</param>
        /// <returns>Returns the updated voice channel.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildVoiceChannel> Modify(GuildVoiceChannelParameters parameters)
        {
            return channelsHttp.ModifyVoiceChannel(Id, parameters);
        }
    }
}
