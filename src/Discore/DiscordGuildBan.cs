namespace Discore
{
    public sealed class DiscordGuildBan
    {
        /// <summary>
        /// Gets the reason for the ban or null if there was no reason.
        /// </summary>
        public string Reason { get; }

        /// <summary>
        /// Gets the user that was banned.
        /// </summary>
        public DiscordUser User { get; }

        internal DiscordGuildBan(DiscordApiData data)
        {
            Reason = data.GetString("reason");
            User = new DiscordUser(false, data.Get("user"));
        }
    }
}
