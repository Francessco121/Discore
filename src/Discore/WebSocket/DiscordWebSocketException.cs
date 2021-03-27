using System;

namespace Discore.WebSocket
{
    /// <summary>
    /// An exception that occurs when interacting with the Discord WebSocket API.
    /// </summary>
    public class DiscordWebSocketException : Exception
    {
        /// <summary>
        /// Gets the type of error that triggered the exception.
        /// </summary>
        public DiscordWebSocketError Error { get; }

        internal DiscordWebSocketException(string message, DiscordWebSocketError error, Exception innerException)
            : base(message)
        {
            Error = error;
        }
    }
}
