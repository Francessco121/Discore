using System.Net;

namespace Discore.Http
{
    public class DiscordHttpClientException : DiscoreException
    {
        public DiscordHttpErrorCode ErrorCode { get; }
        public HttpStatusCode HttpStatusCode { get; }

        internal DiscordHttpClientException(string message, DiscordHttpErrorCode errorCode, HttpStatusCode httpCode)
            : base($"{message} ({errorCode})({(int)errorCode})")
        {
            ErrorCode = errorCode;
            HttpStatusCode = httpCode;
        }
    }
}
