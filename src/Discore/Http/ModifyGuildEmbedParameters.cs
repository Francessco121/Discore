namespace Discore.Http
{
    public class ModifyGuildEmbedParameters
    {
        /// <summary>
        /// Whether this embed is enabled.
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// The embed guild channel ID.
        /// </summary>
        public Snowflake ChannelId { get; set; }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("enabled", Enabled);
            data.Set("channel_id", ChannelId);

            return data;
        }
    }
}
