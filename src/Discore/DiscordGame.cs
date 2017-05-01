namespace Discore
{
    /// <summary>
    /// Representation of the game a user is currently playing.
    /// </summary>
    public sealed class DiscordGame
    {
        /// <summary>
        /// Gets the name of the game.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the type of the game.
        /// </summary>
        public DiscordGameType Type { get; }
        /// <summary>
        /// Gets the URL of the stream when the type is set to "Streaming" and the URL is valid.
        /// Otherwise, returns null.
        /// </summary>
        public string Url { get; }

        internal DiscordGame(DiscordApiData data)
        {
            Name = data.GetString("name");
            Type = (DiscordGameType)(data.GetInteger("type") ?? 0);
            Url = data.GetString("url");
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
