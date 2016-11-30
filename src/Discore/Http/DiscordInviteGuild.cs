namespace Discore.Http
{
    public class DiscordInviteGuild
    {
        /// <summary>
        /// Gets the id of the guild this invite is for.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// Gets the name of the guild.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the hash of the guild splash (or null if none exists).
        /// </summary>
        public string SplashHash { get; }

        public DiscordInviteGuild(DiscordApiData data)
        {
            GuildId = data.GetSnowflake("id").Value;
            Name = data.GetString("name");
            SplashHash = data.GetString("splash_hash");
        }
    }
}
