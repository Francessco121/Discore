using System;
using System.Net;
using System.Text;

namespace Discore.Http
{
    /// <summary>
    /// An exception representing an error sent by the Discord HTTP API.
    /// </summary>
    public class DiscordHttpApiException : Exception
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
            : base(CreateExceptionMessage(message, errorCode, httpCode))
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpCode;
        }

        static string CreateExceptionMessage(string message, DiscordHttpErrorCode errorCode, HttpStatusCode httpCode)
        {
            StringBuilder sb = new StringBuilder();

            if (errorCode == DiscordHttpErrorCode.None)
                sb.Append($"{httpCode}({(int)httpCode}): ");
            else
                sb.Append($"{errorCode}({(int)errorCode}): ");

            sb.Append(message);

            return sb.ToString();
        }
    }
}
