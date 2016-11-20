namespace Discore.Http
{
    public class DiscordInviteGuild : DiscordIdObject
    {
        /// <summary>
        /// Gets the name of the guild.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the hash of the guild splash (or null if none exists).
        /// </summary>
        public string SplashHash { get; }

        public DiscordInviteGuild(DiscordApiData data)
            : base(data)
        {
            Name = data.GetString("name");
            SplashHash = data.GetString("splash_hash");
        }
    }
}
