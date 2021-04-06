using Discore.Http.Internal;
using System;
using System.Net;

#nullable enable

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
        /// The maximum number of requests that can be made until the reset time.
        /// <para>Note: Only set if not a global rate limit.</para>
        /// </summary>
        public int? Limit { get; }
        /// <summary>
        /// Epoch time (seconds since 00:00:00 UTC on January 1, 1970) at which the rate limit resets.
        /// <para>Note: Only set if not a global rate limit.</para>
        /// </summary>
        [Obsolete("Please use ResetHighPrecision instead for millisecond precision.")]
        public ulong? Reset { get; }
        /// <summary>
        /// Epoch time (seconds since 00:00:00 UTC on January 1, 1970) at which the rate limit resets.
        /// <para>Note: Only set if not a global rate limit.</para>
        /// </summary>
        public double? ResetHighPrecision { get; }
        /// <summary>
        /// The time in milliseconds that needs to be waited before sending another request.
        /// </summary>
        public int RetryAfter { get; }

        internal DiscordHttpRateLimitException(RateLimitHeaders rateLimitHeaders, 
            string message, DiscordHttpErrorCode errorCode, HttpStatusCode httpCode) 
            : base(message, errorCode, httpCode)
        {
            IsGlobal = rateLimitHeaders.IsGlobal;
            Limit = rateLimitHeaders.Limit;
            ResetHighPrecision = rateLimitHeaders.Reset;
            RetryAfter = rateLimitHeaders.RetryAfter.GetValueOrDefault(); // Should always be set, but just in case.

#pragma warning disable CS0618 // Type or member is obsolete
            Reset = (ulong?)rateLimitHeaders.Reset;
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}

#nullable restore
