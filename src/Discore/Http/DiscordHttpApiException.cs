using System;
using System.Net;

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
        /// <summary>
        /// Gets the error messages and error codes for each form field, if any.
        /// </summary>
        public DiscordHttpErrorObject? Errors { get; }

        internal DiscordHttpApiException(
            string message, 
            DiscordHttpErrorCode errorCode, 
            HttpStatusCode httpCode, 
            DiscordHttpErrorObject? errors)
            : base(CreateExceptionMessage(message, errorCode, httpCode))
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpCode;
            Errors = errors;
        }

        static string CreateExceptionMessage(string message, DiscordHttpErrorCode errorCode, HttpStatusCode httpCode)
        {
            if (errorCode == DiscordHttpErrorCode.None)
                return $"{httpCode}({(int)httpCode}): {message}";
            else
                return $"{errorCode}({(int)errorCode}): {message}";
        }
    }
}
