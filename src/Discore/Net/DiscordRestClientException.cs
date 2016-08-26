namespace Discore.Net
{
    /// <summary>
    /// An exception thrown by an <see cref="IDiscordRestClient"/> instance.
    /// </summary>
    public class DiscordRestClientException : DiscoreException
    {
        /// <summary>
        /// The error code specified from the <see cref="IDiscordRestClient"/>.
        /// </summary>
        public RestErrorCode ErrorCode { get; }

        /// <summary>
        /// Creates a new <see cref="DiscordRestClientException"/> instance.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="errorCode">The error code that describes this <see cref="DiscordRestClientException"/>.</param>
        public DiscordRestClientException(string message, RestErrorCode errorCode)
            : base($"{message} ({errorCode})")
        {
            ErrorCode = errorCode;
        }
    }
}
