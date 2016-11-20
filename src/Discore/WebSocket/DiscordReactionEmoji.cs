namespace Discore.WebSocket
{
    public sealed class DiscordReactionEmoji : DiscordObject
    {
        /// <summary>
        /// Gets the id of the emoji (if custom emoji).
        /// </summary>
        public Snowflake? Id { get; private set; }

        /// <summary>
        /// Gets the name of the emoji.
        /// </summary>
        public string Name { get; private set; }

        internal DiscordReactionEmoji() { }

        internal override void Update(DiscordApiData data)
        {
            Id = data.GetSnowflake("id");
            Name = data.GetString("name") ?? Name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
