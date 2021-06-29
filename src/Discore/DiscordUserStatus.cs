namespace Discore
{
    /// <summary>
    /// The status of a user.
    /// </summary>
    public enum DiscordUserStatus
    {
        Offline,
        DoNotDisturb,
        Idle,
        Online,
        /// <summary>
        /// Note: This only applies to setting the status of the current bot.
        /// Invisible users will always have the value of <see cref="Offline"/>.
        /// </summary>
        Invisible
    }
}
