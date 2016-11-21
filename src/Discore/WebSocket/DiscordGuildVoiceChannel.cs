using Discore.Http.Net;

namespace Discore.WebSocket
{
    public sealed class DiscordGuildVoiceChannel : DiscordGuildChannel
    {
        /// <summary>
        /// Gets the audio bitrate used for this channel.
        /// </summary>
        public int Bitrate { get; private set; }

        /// <summary>
        /// Gets the maximum number of users that can be connected to this channel simultaneously.
        /// </summary>
        public int UserLimit { get; private set; }

        /// <summary>
        /// Gets a list of all guild members currently in this voice channel.
        /// </summary>
        public DiscordApiCacheIdSet<DiscordGuildMember> Members { get; }

        HttpChannelsEndpoint channelsHttp;

        internal DiscordGuildVoiceChannel(Shard shard, DiscordGuild guild)
            : base(shard, guild, DiscordGuildChannelType.Voice)
        {
            Members = new DiscordApiCacheIdSet<DiscordGuildMember>(guild.Members);

            channelsHttp = shard.Application.InternalHttpApi.Channels;
        }

        /// <summary>
        /// Modifies this voice channel.
        /// Any parameters not specified will be unchanged.
        /// </summary>
        public void Modify(string name = null, int? position = null, int? bitrate = null, int? userLimit = null)
        {
            channelsHttp.Modify(Id, name, position, null, bitrate, userLimit);
        }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            Bitrate = data.GetInteger("bitrate") ?? Bitrate;
            UserLimit = data.GetInteger("user_limit") ?? UserLimit;
        }
    }
}
