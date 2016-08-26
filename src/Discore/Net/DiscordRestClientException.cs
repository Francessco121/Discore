namespace Discore.Net
{
    public class DiscordRestClientException : DiscordioException
    {
        public RestErrorCode ErrorCode;

        public DiscordRestClientException(string message, RestErrorCode errorCode)
            : base($"{message} ({errorCode})")
        {
            ErrorCode = errorCode;
        }
    }
}
