using System;
using System.Net.WebSockets;

namespace Discore
{
    /// <summary>
    /// An exception thrown by a socket connection in Discore.
    /// </summary>
    public class DiscoreSocketException : DiscoreException
    {
        /// <summary>
        /// The error code that caused the socket exception.
        /// </summary>
        public WebSocketCloseStatus ErrorCode { get; }

        /// <summary>
        /// Creates a <see cref="DiscoreSocketException"/>
        /// </summary>
        /// <param name="message">The message of the <see cref="DiscoreSocketException"/>.</param>
        /// <param name="errorCode">The error code that caused the exception.</param>
        public DiscoreSocketException(string message, WebSocketCloseStatus errorCode)
            : base($"{message} ({(int)errorCode})")
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Creates a <see cref="DiscoreSocketException"/>
        /// </summary>
        /// <param name="message">The message of the <see cref="DiscoreSocketException"/>.</param>
        /// <param name="errorCode">The error code that caused the exception.</param>
        /// <param name="innerException">The inner exception.</param>
        public DiscoreSocketException(string message, WebSocketCloseStatus errorCode, Exception innerException)
            : base($"{message} ({(int)errorCode})", innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
