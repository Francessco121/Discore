namespace Discore.WebSocket
{
    public class DiscordInviteGuild : DiscordObject
    {
        public DiscordGuild Guild { get; private set; }

        /// <summary>
        /// Gets the name of the guild.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the hash of the guild splash (or null if none exists).
        /// </summary>
        public string SplashHash { get; private set; }

        Shard shard;

        internal DiscordInviteGuild(Shard shard)
        {
            this.shard = shard;
        }

        internal override void Update(DiscordApiData data)
        {
            Snowflake id = data.GetSnowflake("id").Value;
            Guild = shard.Guilds.Get(id);

            Name = data.GetString("name");
            SplashHash = data.GetString("splash_hash");
        }
    }
}
