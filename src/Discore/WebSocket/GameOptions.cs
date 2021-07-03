namespace Discore.WebSocket
{
    public class GameOptions
    {
        /// <summary>
        /// The name of the game.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The type of the game. Defaults to <see cref="DiscordActivityType.Game"/>.
        /// </summary>
        public DiscordActivityType Type { get; set; } = DiscordActivityType.Game;

        /// <summary>
        /// The URL of the stream. <see cref="Type"/> must be <see cref="DiscordActivityType.Streaming"/>
        /// for this to take effect. Defaults to null.
        /// </summary>
        public string? Url { get; set; }

        public GameOptions() { }
        public GameOptions(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Sets the name of the game.
        /// </summary>
        public GameOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the type of game.
        /// </summary>
        public GameOptions SetType(DiscordActivityType type)
        {
            Type = type;
            return this;
        }

        /// <summary>
        /// Sets the URL of the stream. <see cref="Type"/> must be <see cref="DiscordActivityType.Streaming"/>
        /// for this to take effect.
        /// </summary>
        public GameOptions SetUrl(string url)
        {
            Url = url;
            return this;
        }
    }
}
