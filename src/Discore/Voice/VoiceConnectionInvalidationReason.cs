namespace Discore.Voice
{
    /// <summary>
    /// Reasons for voice connections being invalidated.
    /// </summary>
    public enum VoiceConnectionInvalidationReason
    {
        /// <summary>
        /// The voice connection disconnected normally.
        /// </summary>
        Normal,
        /// <summary>
        /// The voice connection was disconnected because the bot was removed from the
        /// guild the voice connection was for.
        /// </summary>
        BotRemovedFromGuild,
        /// <summary>
        /// The voice connection timed out while waiting for a response from the Discord API.
        /// </summary>
        TimedOut,
        /// <summary>
        /// The voice connection encountered a fatal error.
        /// </summary>
        Error
    }
}
