namespace Discore.Http
{
    /// <summary>
    /// Represents a strategy used when retrieving guilds from the Discord API.
    /// </summary>
    public enum GuildGetStrategy
    {
        /// <summary>
        /// Will return guilds before the guild ID.
        /// </summary>
        Before,
        /// <summary>
        /// Will return guilds after the guild ID.
        /// </summary>
        After
    }
}
