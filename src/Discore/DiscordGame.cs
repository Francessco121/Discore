namespace Discore
{
    /// <summary>
    /// Representation of a game a <see cref="DiscordUser"/> is currently playing.
    /// </summary>
    public class DiscordGame : IDiscordObject
    {
        /// <summary>
        /// Gets the name of the game.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets the type of the game.
        /// </summary>
        public DiscordGameType Type { get; set; }

        /// <summary>
        /// Updates this game with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update game user with.</param>
        public void Update(DiscordApiData data)
        {
            Name = data.GetString("name") ?? Name;

            int? type = data.GetInteger("type");
            if (type.HasValue)
                Type = (DiscordGameType)type.Value;
        }
    }
}
