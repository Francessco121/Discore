namespace Discore
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

        internal DiscordGuildVoiceChannel(Shard shard, DiscordGuild guild)
            : base(shard, guild, DiscordGuildChannelType.Voice)
        {
            Members = new DiscordApiCacheIdSet<DiscordGuildMember>(guild.Members);
        }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            Bitrate = data.GetInteger("bitrate") ?? Bitrate;
            UserLimit = data.GetInteger("user_limit") ?? UserLimit;
        }
    }
}
