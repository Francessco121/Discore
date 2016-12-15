using System.Net;

namespace Discore.Http
{
    /// <summary>
    /// An exception representing an error sent by the Discord HTTP API.
    /// </summary>
    public class DiscordHttpApiException : DiscoreException
    {
        /// <summary>
        /// Gets the custom Discord HTTP error code.
        /// </summary>
        public DiscordHttpErrorCode ErrorCode { get; }
        /// <summary>
        /// Gets the HTTP status code associated with the error.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; }

        internal DiscordHttpApiException(string message, DiscordHttpErrorCode errorCode, HttpStatusCode httpCode)
            : base($"{message} ({errorCode})({(int)errorCode})")
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpCode;
        }
    }
}
