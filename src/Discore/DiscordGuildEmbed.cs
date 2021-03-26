namespace Discore
{
    public sealed class DiscordGuildEmbed
    {
        /// <summary>
        /// Gets whether this embed is enabled.
        /// </summary>
        public bool Enabled { get; }
        /// <summary>
        /// Gets the embed channel ID.
        /// </summary>
        public Snowflake ChannelId { get; }
        /// <summary>
        /// Gets the ID of the guild this embed is for.
        /// </summary>
        public Snowflake GuildId { get; }

        internal DiscordGuildEmbed(Snowflake guildId, DiscordApiData data)
        {
            GuildId = guildId;

            Enabled = data.GetBoolean("enabled").Value;
            ChannelId = data.GetSnowflake("channel_id").Value;
        }
    }
}
