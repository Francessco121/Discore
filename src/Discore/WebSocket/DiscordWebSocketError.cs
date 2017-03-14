namespace Discore.WebSocket
{
    /// <summary>
    /// Types of errors that trigger a <see cref="DiscordWebSocketException"/>.
    /// </summary>
    public enum DiscordWebSocketError
    {
        /// <summary>
        /// An unexpected error occured while interacting with the Discord WebSocket API.
        /// Should never happen, indicates a type of error that should be handled in Discore.
        /// </summary>
        Unexpected,
        /// <summary>
        /// The WebSocket connection was closed while sending data.
        /// </summary>
        ConnectionClosed
    }
}
