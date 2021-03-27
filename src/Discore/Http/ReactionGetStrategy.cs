namespace Discore.Http
{
    /// <summary>
    /// Represents a pagination strategy used when retrieving reactions from the Discord API.
    /// </summary>
    public enum ReactionGetStrategy
    {
        /// <summary>
        /// Will return reactions before the user ID.
        /// </summary>
        Before,
        /// <summary>
        /// Will return reactions after the user ID.
        /// </summary>
        After
    }
}
