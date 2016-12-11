namespace Discore
{
    public sealed class DiscordGuildEmbed
    {
        /// <summary>
        /// Gets whether this embed is enabled.
        /// </summary>
        public bool Enabled { get; }
        /// <summary>
        /// Gets the embed channel id.
        /// </summary>
        public Snowflake ChannelId { get; }

        internal DiscordGuildEmbed(DiscordApiData data)
        {
            Enabled = data.GetBoolean("enabled").Value;
            ChannelId = data.GetSnowflake("channel_id").Value;
        }
    }
}
