namespace Discore
{
    public class DiscordActivityEmoji
    {
        /// <summary>
        /// Gets the name of the emoji.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the ID of the emoji.
        /// </summary>
        public Snowflake? Id { get; }

        /// <summary>
        /// Gets whether this emoji is animated.
        /// </summary>
        public bool Animated { get; }

        internal DiscordActivityEmoji(DiscordApiData data)
        {
            Name = data.GetString("name");
            Id = data.GetSnowflake("id");
            Animated = data.GetBoolean("animated") ?? false;
        }
    }
}
