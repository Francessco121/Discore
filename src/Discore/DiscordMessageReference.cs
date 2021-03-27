namespace Discore
{
    public class DiscordMessageReference
    {
        /// <summary>
        /// Gets the ID of the originating message.
        /// </summary>
        public Snowflake? MessageId { get; }

        /// <summary>
        /// Gets the ID of the originating message's channel.
        /// </summary>
        public Snowflake ChannelId { get; }

        /// <summary>
        /// Gets the ID of the originating message's guild.
        /// </summary>
        public Snowflake? GuildId { get; }

        internal DiscordMessageReference(DiscordApiData data)
        {
            MessageId = data.GetSnowflake("message_id");
            ChannelId = data.GetSnowflake("channel_id").GetValueOrDefault();
            GuildId = data.GetSnowflake("guild_id");
        }
    }
}
