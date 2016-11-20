namespace Discore.Http
{
    public class DiscordReactionEmoji
    {
        /// <summary>
        /// Gets the id of the emoji (if custom emoji).
        /// </summary>
        public Snowflake? Id { get; }

        /// <summary>
        /// Gets the name of the emoji.
        /// </summary>
        public string Name { get; }

        public DiscordReactionEmoji(DiscordApiData data)
        {
            Id = data.GetSnowflake("id");
            Name = data.GetString("name");
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
