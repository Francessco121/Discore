namespace Discore
{
    /// <summary>
    /// Represents a strategy used when retrieving messages from the Discord API.
    /// </summary>
    public enum MessageGetStrategy
    {
        /// <summary>
        /// Will return messages before and after the base message.
        /// </summary>
        Around,
        /// <summary>
        /// Will return messages before the base message.
        /// </summary>
        Before,
        /// <summary>
        /// Will return message after the base message.
        /// </summary>
        After
    }
}
