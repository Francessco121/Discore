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
        /// The voice connection timed out while waiting for a response from the Discord API.
        /// </summary>
        TimedOut,
        /// <summary>
        /// The voice connection encountered a fatal error.
        /// </summary>
        Error,
        /// <summary>
        /// The DLLs for libopus and/or libsodium could not be found.
        /// </summary>
        DllNotFound
    }
}
