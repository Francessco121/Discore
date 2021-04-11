namespace Discore.WebSocket
{
    public class StatusOptions
    {
        /// <summary>
        /// The status of the bot. Defaults to <see cref="DiscordUserStatus.Online"/>.
        /// </summary>
        public DiscordUserStatus Status { get; set; } = DiscordUserStatus.Online;

        /// <summary>
        /// Whether the bot is AFK. Defaults to false.
        /// </summary>
        public bool Afk { get; set; } = false;

        /// <summary>
        /// Unix time (in milliseconds) of when the bot went idle,
        /// or null if the bot is not idle. Defaults to null.
        /// </summary>
        public int? AfkSince { get; set; }

        /// <summary>
        /// The "game" the bot is currently playing, or null if the bot
        /// is not "playing" anything. Defaults to null.
        /// </summary>
        public GameOptions? Game { get; set; }

        public StatusOptions() { }

        /// <summary>
        /// Sets the status of the bot.
        /// </summary>
        public StatusOptions SetStatus(DiscordUserStatus status)
        {
            Status = status;
            return this;
        }

        /// <summary>
        /// Sets whether the bot is AFK.
        /// </summary>
        public StatusOptions SetAfk(bool afk)
        {
            Afk = afk;
            return this;
        }

        /// <summary>
        /// Sets the unix time (in milliseconds) of when the bot went idle,
        /// or null if the bot is not idle.
        /// </summary>
        public StatusOptions SetAfkSince(int? afkSince)
        {
            AfkSince = afkSince;
            return this;
        }

        /// <summary>
        /// Sets the "game" the bot is currently playing, or null if the bot
        /// is not "playing" anything.
        /// </summary>
        public StatusOptions SetGame(GameOptions game)
        {
            Game = game;
            return this;
        }
    }
}
