namespace Discore.Net
{
    /// <summary>
    /// Specifies the way messages should be retrieved from an <see cref="IDiscordRestMessagesService"/> instance.
    /// </summary>
    public enum DiscordMessageGetStrategy
    {
        /// <summary>
        /// Get messages around the message ID.
        /// </summary>
        Around,
        /// <summary>
        /// Get messages before the message ID.
        /// </summary>
        Before,
        /// <summary>
        /// Get messages after the message ID.
        /// </summary>
        After
    }
}
