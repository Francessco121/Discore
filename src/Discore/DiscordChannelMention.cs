namespace Discore
{
    public class DiscordChannelMention : DiscordIdEntity
    {
        /// <summary>
        /// Gets the ID of the guild containing the channel.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// Gets the channel type.
        /// </summary>
        public DiscordChannelType Type { get; }

        /// <summary>
        /// Gets the name of the channel.
        /// </summary>
        public string Name { get; }

        internal DiscordChannelMention(DiscordApiData data)
            : base(data)
        {
            GuildId = data.GetSnowflake("guild_id").GetValueOrDefault();
            Type = (DiscordChannelType)(data.GetInteger("type") ?? 0);
            Name = data.GetString("name");
        }
    }
}
