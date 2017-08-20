using Discore.Http;
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

        DiscordHttpClient http;

        internal DiscordGuildVoiceChannel(DiscordHttpClient http, DiscordApiData data, Snowflake? guildId = null)
            : base(http, data, DiscordChannelType.GuildVoice, guildId)
        {
            this.http = http;

            Bitrate = data.GetInteger("bitrate").Value;
            UserLimit = data.GetInteger("user_limit").Value;
        }

        /// <summary>
        /// Modifies this voice channel's settings.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="options">A set of options to modify the channel with</param>
        /// <returns>Returns the updated voice channel.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildVoiceChannel> Modify(GuildVoiceChannelOptions options)
        {
            return http.ModifyVoiceChannel(Id, options);
        }
    }
}
