namespace Discore.Net
{
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
