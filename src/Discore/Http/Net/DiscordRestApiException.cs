using System.Net;

namespace Discore.Http.Net
{
    public class DiscordRestClientException : DiscoreException
    {
        public DiscordRestErrorCode ErrorCode { get; }
        public HttpStatusCode HttpStatusCode { get; }

        internal DiscordRestClientException(string message, DiscordRestErrorCode errorCode, HttpStatusCode httpCode)
            : base($"{message} ({errorCode})")
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpCode;
        }
    }
}
