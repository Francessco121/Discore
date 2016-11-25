using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class RestClientRateLimitManager
    {
        /// <summary>
        /// https://github.com/hammerandchisel/discord-api-docs/blob/master/docs/topics/RATE_LIMITS.md
        /// </summary>
        /// <remarks>
        /// The way this works is, on RateLimiter.Wait(), if there is no remaining calls for the specified action 
        /// and the reset timer hasn't finished, we enter the waiting state. If this is the first rate limiter to 
        /// wait on this action, it goes ahead to wait the remaining reset timer seconds. If it isn't the first,
        /// it enters a RateLimiterBlocker into the call queue and waits on that. When the currently waiting call 
        /// finishes, and invokes RateLimiter.Update(), the first blocker in the queue will be cleared to continue 
        /// and attempt to wait the remaining reset timer time, and the cycle continues from there.
        /// </remarks>
        class RateLimiter
        {
            class RateLimitBlocker
            {
                ManualResetEvent resetEvent;

                public RateLimitBlocker()
                {
                    resetEvent = new ManualResetEvent(false);
                }

                public void Continue()
                {
                    resetEvent.Set();
                }

                public void Wait()
                {
                    resetEvent.WaitOne();
                }
            }

            public int Limit { get { return limit; } }

            int resetTime;
            int remaining;
            int limit;

            ConcurrentQueue<RateLimitBlocker> queuedCalls;
            bool waiting;

            public RateLimiter()
            {
                queuedCalls = new ConcurrentQueue<RateLimitBlocker>();
            }

            public void Update(HttpResponseMessage response)
            {
                IEnumerable<string> limitValues;
                if (response.Headers.TryGetValues("X-RateLimit-Limit", out limitValues))
                {
                    string limit = limitValues.FirstOrDefault();

                    int limitInt;
                    if (!string.IsNullOrWhiteSpace(limit) && int.TryParse(limit, out limitInt))
                        this.limit = limitInt;
                }

                IEnumerable<string> remainingValues;
                if (response.Headers.TryGetValues("X-RateLimit-Remaining", out remainingValues))
                {
                    string remaining = remainingValues.FirstOrDefault();

                    int remainingInt;
                    if (!string.IsNullOrWhiteSpace(remaining) && int.TryParse(remaining, out remainingInt))
                        this.remaining = remainingInt;
                }

                IEnumerable<string> resetValues;
                if (response.Headers.TryGetValues("X-RateLimit-Reset", out resetValues))
                {
                    string reset = resetValues.FirstOrDefault();

                    int resetTimeInt;
                    if (!string.IsNullOrWhiteSpace(reset) && int.TryParse(reset, out resetTimeInt))
                        this.resetTime = resetTimeInt;
                }

                RateLimitBlocker blocker;
                if (queuedCalls.TryDequeue(out blocker))
                    blocker.Continue();
            }

            public async Task Wait()
            {
                if (remaining == 0)
                {
                    if (waiting || queuedCalls.Count > 0)
                    {
                        RateLimitBlocker blocker = new RateLimitBlocker();
                        queuedCalls.Enqueue(blocker);

                        blocker.Wait();
                    }

                    TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
                    int epochSeconds = (int)t.TotalSeconds;

                    if (resetTime > epochSeconds)
                    {
                        waiting = true;
                        int waitTime = (resetTime - epochSeconds) * 1000;
                        await Task.Delay(waitTime);
                        waiting = false;
                    }
                }
            }
        }

        ConcurrentDictionary<string, RateLimiter> rateLimiters;

        public RestClientRateLimitManager()
        {
            rateLimiters = new ConcurrentDictionary<string, RateLimiter>();
        }

        public async Task AwaitRateLimiter(string action)
        {
            RateLimiter limiter;
            if (rateLimiters.TryGetValue(action, out limiter))
                await limiter.Wait();
        }

        public void UpdateRateLimiter(string action, HttpResponseMessage response)
        {
            RateLimiter limiter;
            if (!rateLimiters.TryGetValue(action, out limiter))
            {
                limiter = new RateLimiter();
                if (!rateLimiters.TryAdd(action, limiter))
                    limiter = rateLimiters[action];
            }

            limiter.Update(response);
        }
    }
}
