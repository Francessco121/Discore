namespace Discore
{
    public class DiscordActivitySecrets
    {
        /// <summary>
        /// Gets the secret for joining a party. May be null.
        /// </summary>
        public string Join { get; }

        /// <summary>
        /// Gets the secret for spectating a game. May be null.
        /// </summary>
        public string Spectate { get; }

        /// <summary>
        /// Gets the secret for a specific instanced match. May be null.
        /// </summary>
        public string Match { get; }

        internal DiscordActivitySecrets(DiscordApiData data)
        {
            Join = data.GetString("join");
            Spectate = data.GetString("spectate");
            Match = data.GetString("match");
        }
    }
}
