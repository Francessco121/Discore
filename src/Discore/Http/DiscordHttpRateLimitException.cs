using Discore.Http.Internal;
using System.Net;

namespace Discore.Http
{
    /// <summary>
    /// An exception representing a 429 error sent by the Discord HTTP API.
    /// </summary>
    public class DiscordHttpRateLimitException : DiscordHttpApiException
    {
        /// <summary>
        /// Whether this is a global rate limit.
        /// </summary>
        public bool IsGlobal { get; }
        /// <summary>
        /// The maximum number of requests that can be made between rate limit resets.
        /// This is the request count that was exceeded.
        /// <para>Note: Only set if not a global rate limit.</para>
        /// </summary>
        public int? Limit { get; }
        /// <summary>
        /// Epoch time (seconds since 00:00:00 UTC on January 1, 1970) at which the rate limit resets.
        /// <para>Note: Only set if not a global rate limit.</para>
        /// </summary>
        public double? Reset { get; }
        /// <summary>
        /// The time in seconds that needs to be waited before sending another request.
        /// </summary>
        public double RetryAfter { get; }
        /// <summary>
        /// If set, a unique string denoting the rate limit being encountered.
        /// <para>Note: Only set if not a global rate limit.</para>
        /// </summary>
        public string? Bucket { get; }

        internal DiscordHttpRateLimitException(
            RateLimitHeaders rateLimitHeaders, 
            string message, 
            DiscordHttpErrorCode errorCode, 
            HttpStatusCode httpCode,
            DiscordHttpErrorObject? errors) 
            : base(message, errorCode, httpCode, errors)
        {
            IsGlobal = rateLimitHeaders.IsGlobal;
            Limit = rateLimitHeaders.Limit;
            Reset = rateLimitHeaders.Reset;
            RetryAfter = rateLimitHeaders.ResetAfter ?? rateLimitHeaders.RetryAfter.GetValueOrDefault(); // Should always be set, but just in case.
            Bucket = rateLimitHeaders.Bucket;
        }
    }
}
