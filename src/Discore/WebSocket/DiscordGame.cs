namespace Discore.WebSocket
{
    /// <summary>
    /// Representation of the game a user is currently playing.
    /// </summary>
    public sealed class DiscordGame : DiscordObject
    {
        /// <summary>
        /// Gets the name of the game.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets the type of the game.
        /// </summary>
        public DiscordGameType Type { get; set; }

        internal DiscordGame() { }

        internal override void Update(DiscordApiData data)
        {
            Name = data.GetString("name") ?? Name;

            int? type = data.GetInteger("type");
            if (type.HasValue)
                Type = (DiscordGameType)type.Value;
        }

        public override string ToString()
        {
            return $"Game: {Name}, Type: {Type}";
        }
    }
}
