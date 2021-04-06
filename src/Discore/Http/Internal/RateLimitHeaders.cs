using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

#nullable enable

namespace Discore.Http.Internal
{
    class RateLimitHeaders
    {
        /// <summary>
        /// X-RateLimit-Global. Whether this is a global rate limit. Can only be true if response status is 429.
        /// </summary>
        public bool IsGlobal { get; }
        /// <summary>
        /// X-RateLimit-Limit. The maximum number of requests that can be made until the reset time.
        /// </summary>
        public int? Limit { get; }
        /// <summary>
        /// X-RateLimit-Remaining. The number of remaining requests that can be made.
        /// </summary>
        public int? Remaining { get; }
        /// <summary>
        /// X-RateLimit-Reset. Epoch time (seconds since 00:00:00 UTC on January 1, 1970) at which the rate limit resets.
        /// </summary>
        public double? Reset { get; }
        /// <summary>
        /// Retry-After. If set, the time in milliseconds that needs to be waited before sending another request.
        /// </summary>
        public int? RetryAfter { get; }
        /// <summary>
        /// X-RateLimit-Bucket. If set, a unique string denoting the rate limit being encountered (non-inclusive of major parameters in the route path).
        /// </summary>
        public string? Bucket { get; }

        private RateLimitHeaders(bool isGlobal, int? retryAfter)
        {
            IsGlobal = isGlobal;
            RetryAfter = retryAfter;
        }

        private RateLimitHeaders(bool isGlobal, int limit, int remaining, double reset, int? retryAfter, string? bucket)
        {
            IsGlobal = isGlobal;
            Limit = limit;
            Remaining = remaining;
            Reset = reset;
            RetryAfter = retryAfter;
            Bucket = bucket;
        }

        /// <summary>
        /// Extracts rate limit headers from the given HTTP response headers.
        /// Returns null if no rate limit headers are present.
        /// </summary>
        public static RateLimitHeaders? ParseOrNull(HttpResponseHeaders headers)
        {
            bool isGlobal = headers.Contains("X-RateLimit-Global");

            int? retryAfterHeader = null;

            IEnumerable<string> retryAfterValues;
            if (headers.TryGetValues("Retry-After", out retryAfterValues))
            {
                string retryAfterStr = retryAfterValues.FirstOrDefault();

                int retryAfter;
                if (!string.IsNullOrWhiteSpace(retryAfterStr) && int.TryParse(retryAfterStr, out retryAfter))
                    retryAfterHeader = retryAfter;
            }

            if (!isGlobal)
            {
                int? limitHeader = null, remainingHeader = null;
                double? resetTimeHeader = null;
                string? bucket = null;

                IEnumerable<string> limitValues;
                if (headers.TryGetValues("X-RateLimit-Limit", out limitValues))
                {
                    string limit = limitValues.FirstOrDefault();

                    int limitInt;
                    if (!string.IsNullOrWhiteSpace(limit) && int.TryParse(limit, out limitInt))
                        limitHeader = limitInt;
                }

                IEnumerable<string> remainingValues;
                if (headers.TryGetValues("X-RateLimit-Remaining", out remainingValues))
                {
                    string remainingStr = remainingValues.FirstOrDefault();

                    int remaining;
                    if (!string.IsNullOrWhiteSpace(remainingStr) && int.TryParse(remainingStr, out remaining))
                        remainingHeader = remaining;
                }

                IEnumerable<string> resetValues;
                if (headers.TryGetValues("X-RateLimit-Reset", out resetValues))
                {
                    string resetTimeStr = resetValues.FirstOrDefault();

                    double resetTime;
                    if (!string.IsNullOrWhiteSpace(resetTimeStr) && double.TryParse(resetTimeStr, out resetTime))
                        resetTimeHeader = resetTime;
                }

                IEnumerable<string> bucketValues;
                if (headers.TryGetValues("X-RateLimit-Bucket", out bucketValues))
                {
                    bucket = bucketValues.FirstOrDefault();
                }

                if (limitHeader.HasValue && remainingHeader.HasValue && resetTimeHeader.HasValue)
                {
                    return new RateLimitHeaders(isGlobal,
                        limitHeader.Value, remainingHeader.Value, resetTimeHeader.Value, retryAfterHeader, bucket);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new RateLimitHeaders(isGlobal, retryAfterHeader);
            }
        }
    }
}

#nullable restore
