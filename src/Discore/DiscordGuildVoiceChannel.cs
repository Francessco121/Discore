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

        /// <summary>
        /// Gets the ID of the parent category channel or null if the channel is not in a category.
        /// </summary>
        public Snowflake? ParentId { get; }

        internal DiscordGuildVoiceChannel(DiscordApiData data, Snowflake? guildId = null)
            : base(data, DiscordChannelType.GuildVoice, guildId)
        {
            Bitrate = data.GetInteger("bitrate").Value;
            UserLimit = data.GetInteger("user_limit").Value;
            ParentId = data.GetSnowflake("parent_id");
        }
    }
}
