using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Http.Net
{
    /// <summary>
    /// Represents a single rate limitable HTTP API route.
    /// </summary>
    class RateLimitRoute
    {
        int remaining;
        int resetAt;
        int limit;

        bool usedLocalUpdate;

        AsyncManualResetEvent globalRateLimitResetEvent;
        int exceedResetAt;

        AsyncMonitor monitor;

        public RateLimitRoute(AsyncManualResetEvent globalRateLimitResetEvent)
        {
            this.globalRateLimitResetEvent = globalRateLimitResetEvent;

            monitor = new AsyncMonitor();
        }

        /// <summary>
        /// Updates this rate limit section with the rate limit values in 
        /// the provided response headers.
        /// </summary>
        public async Task Update(HttpResponseHeaders headers)
        {
            int? limitHeader = null, remainingHeader = null, resetTimeHeader = null;

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
                string remaining = remainingValues.FirstOrDefault();

                int remainingInt;
                if (!string.IsNullOrWhiteSpace(remaining) && int.TryParse(remaining, out remainingInt))
                    remainingHeader = remainingInt;
            }

            IEnumerable<string> resetValues;
            if (headers.TryGetValues("X-RateLimit-Reset", out resetValues))
            {
                string resetTime = resetValues.FirstOrDefault();

                int resetTimeInt;
                if (!string.IsNullOrWhiteSpace(resetTime) && int.TryParse(resetTime, out resetTimeInt))
                    resetTimeHeader = resetTimeInt;
            }

            if (limitHeader.HasValue && remainingHeader.HasValue && resetTimeHeader.HasValue)
            {
                /* Only update local values:
                 * a) If reset time changed, otherwise keep handling locally.
                 *    This avoids remote responses overwriting the remaining value with an invalid value.
                 *    For instance: if we send off 3 requests where the limit is also 3 per time frame,
                 *    the first response will say (if the time hasn't rolled over) that we have 2 requests
                 *    remaining, even though locally we know that's not true.
                 * b) The API is specifying a smaller remaining count than what we have locally. Most of
                 *    the time the local values should be accurate or at least behind, but occasionally
                 *    due to the concurrent nature of web requests, we can get ahead.
                 * c) The API is specifying a different limit value. The actual limit values are subject
                 *    to change whenever, we should move to the changed limits as soon as possible.
                */
                if (resetTimeHeader.Value > resetAt || remainingHeader.Value < remaining || limitHeader.Value != limit)
                {
                    // Set new values from header
                    limit = limitHeader.Value;
                    resetAt = resetTimeHeader.Value;

                    Interlocked.Exchange(ref remaining, remainingHeader.Value);

                    // Allow a new local update.
                    usedLocalUpdate = false;

                    // Let any waiting tasks that need new header values continue.
                    using (await monitor.EnterAsync().ConfigureAwait(false))
                    {
                        monitor.PulseAll(); 
                    }
                }
            }
        }

        /// <summary>
        /// Forces this rate limit section to wait the Retry-After value provided from a 429 response.
        /// </summary>
        public void ExceededRateLimit(HttpResponseHeaders headers)
        {
            remaining = 0;

            IEnumerable<string> retryAfterValues;
            if (headers.TryGetValues("Retry-After", out retryAfterValues))
            {
                string retryAfterStr = retryAfterValues.FirstOrDefault();

                int retryAfter;
                if (!string.IsNullOrWhiteSpace(retryAfterStr) && int.TryParse(retryAfterStr, out retryAfter))
                {
                    TimeSpan t = (DateTime.UtcNow + TimeSpan.FromMilliseconds(retryAfter)) - new DateTime(1970, 1, 1);
                    int epochSeconds = (int)Math.Ceiling(t.TotalSeconds);

                    exceedResetAt = epochSeconds;
                }
            }
        }

        async Task WaitExceedReset()
        {
            // Wait global rate limiter
            await globalRateLimitResetEvent.WaitAsync().ConfigureAwait(false);

            // Wait local rate limiter
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int epochSeconds = (int)t.TotalSeconds;

            if (epochSeconds < exceedResetAt)
            {
                // Wait exceed time.
                int waitTime = (exceedResetAt - epochSeconds + 1) * 1000;
                await Task.Delay(waitTime).ConfigureAwait(false);
            }
        }

        async Task WaitMonitor()
        {
            using (await monitor.EnterAsync().ConfigureAwait(false))
            {
                await monitor.WaitAsync().ConfigureAwait(false);
            }
        }

        public async Task Wait()
        {
            bool retry;

            do
            {
                retry = false;
                await WaitExceedReset().ConfigureAwait(false);

                if (remaining == 0)
                {
                    TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                    int epochSeconds = (int)t.TotalSeconds;

                    if (resetAt > epochSeconds)
                    {
                        // Wait until reset time.
                        int waitTime = (resetAt - epochSeconds + 1) * 1000;
                        await Task.Delay(waitTime).ConfigureAwait(false);

                        retry = true;
                    }
                    else
                    {
                        if (!usedLocalUpdate)
                        {
                            // Use a local update to keep the flow going,
                            // we can only safely do this once however
                            // until we get a new reset time.

                            usedLocalUpdate = true;
                            Interlocked.Exchange(ref remaining, limit);
                        }
                        else
                        {
                            // Wait for either new header values, or a 2s timeout.
                            await Task.WhenAny(WaitMonitor(), Task.Delay(2000)).ConfigureAwait(false);

                            if (remaining == 0)
                            {
                                // Didn't receive any updates, so we have to improvise.
                                Interlocked.Exchange(ref remaining, limit);
                            }

                            retry = true;
                        }
                    }
                }
                else
                {
                    // No rate limiting needed.
                    Interlocked.Decrement(ref remaining);
                }

            } while (retry);

            await WaitExceedReset().ConfigureAwait(false);
        }
    }
}
