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
        public string Name { get; set; }
        /// <summary>
        /// Gets the type of the game.
        /// </summary>
        public DiscordGameType Type { get; set; }

        internal DiscordGame(DiscordApiData data)
        {
            Name = data.GetString("name");
            Type = (DiscordGameType)data.GetInteger("type").Value;
        }

        public override string ToString()
        {
            return $"Game: {Name}, Type: {Type}";
        }
    }
}
