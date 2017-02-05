using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class ThrottleRateLimitHandler : RateLimitHandler, IDisposable
    {
        ConcurrentQueue<AsyncManualResetEvent> queue;
        AsyncManualResetEvent queueAddEvent;
        Task queueTask;
        bool isProcessing;

        ulong exceedResetEpoch;

        AsyncLock updateLock;

        AsyncManualResetEvent newResetTimeEvent;

        int limit;
        int remaining;
        ulong reset;
        bool performedLocalReset;

        public ThrottleRateLimitHandler()
        {
            queue = new ConcurrentQueue<AsyncManualResetEvent>();
            queueAddEvent = new AsyncManualResetEvent();

            newResetTimeEvent = new AsyncManualResetEvent();
            updateLock = new AsyncLock();

            queueTask = ProcessQueue();
        }

        public override void ExceededRateLimit(RateLimitHeaders headers)
        {
            exceedResetEpoch = GetEpochFromNow((int)Math.Ceiling(headers.RetryAfter.Value / 1000f));
        }

        public override async Task UpdateValues(RateLimitHeaders headers)
        {
            using (await updateLock.LockAsync().ConfigureAwait(false))
            {
                if (headers.Reset > reset || headers.Limit != limit)
                {
                    limit = headers.Limit;
                    reset = headers.Reset;

                    // If we performed a local reset, don't bump up the remaining from a late response.
                    if (!performedLocalReset || headers.Remaining < remaining)
                        remaining = headers.Remaining;

                    performedLocalReset = false;

                    newResetTimeEvent.Set();
                }

                if (headers.Remaining < remaining)
                    remaining = headers.Remaining;
            }
        }

        public override Task Wait()
        {
            AsyncManualResetEvent evt = new AsyncManualResetEvent();
            queue.Enqueue(evt);

            queueAddEvent.Set();

            return evt.WaitAsync();
        }

        async Task ProcessQueue()
        {
            isProcessing = true;

            while (isProcessing)
            {
                // Wait for item in queue
                await queueAddEvent.WaitAsync().ConfigureAwait(false);

                // Process item
                AsyncManualResetEvent evt;
                if (queue.TryDequeue(out evt))
                {
                    bool canContinue = false;

                    // Continue if we have a request remaining
                    using (await updateLock.LockAsync().ConfigureAwait(false))
                    {
                        if (remaining > 0)
                        {
                            --remaining;
                            canContinue = true;
                        }
                    }

                    if (!canContinue)
                    {
                        // Continue if we haven't locally reset and we have passed the reset time
                        using (await updateLock.LockAsync().ConfigureAwait(false))
                        {
                            int timeUntilReset = GetSecondsUntilEpoch(reset) + 1000;
                            if (!performedLocalReset && timeUntilReset <= 0)
                            {
                                performedLocalReset = true;
                                remaining = limit - 1;
                                canContinue = true;
                            }
                        }
                    }

                    if (!canContinue)
                    {
                        // Continue if we haven't locally reset, and after we wait the reset time.
                        int timeUntilResetMs;
                        bool shouldWait = false;
                        using (await updateLock.LockAsync().ConfigureAwait(false))
                        {
                            timeUntilResetMs = GetSecondsUntilEpoch(reset) * 1000 + 1000;
                            shouldWait = !performedLocalReset && timeUntilResetMs > 0;
                        }

                        if (shouldWait)
                        {
                            // Wait until reset time
                            await Task.Delay(timeUntilResetMs).ConfigureAwait(false);

                            using (await updateLock.LockAsync().ConfigureAwait(false))
                            {
                                if (remaining == 0)
                                {
                                    // No updates have done a remote reset so locally reset.
                                    performedLocalReset = true;
                                    remaining = limit - 1;
                                }
                                else
                                    // Update has done a remote reset so just decrement the remaining
                                    --remaining;

                                canContinue = true;
                            }
                        }
                    }

                    if (!canContinue)
                    {
                        // Wait for either a remote reset or a 5 second timeout.
                        await Task.WhenAny(newResetTimeEvent.WaitAsync(), Task.Delay(5000)).ConfigureAwait(false);

                        // Even if a remote reset occured, the remaining requests may still be zero.
                        // If this is true, we need to wait until the next reset time.
                        int timeUntilResetMs = 0;
                        bool shouldWait = false;
                        using (await updateLock.LockAsync().ConfigureAwait(false))
                        {
                            // performedLocalReset will be false here if a remote reset occured.
                            if (!performedLocalReset && remaining == 0)
                            {
                                timeUntilResetMs = GetSecondsUntilEpoch(reset) * 1000 + 1000;
                                shouldWait = timeUntilResetMs > 0;
                            }
                        }

                        if (shouldWait)
                            // Wait until the new reset time.
                            await Task.Delay(timeUntilResetMs).ConfigureAwait(false);

                        // If no remote reset occured, then we won't know the situation until we let 
                        // another request through, so no further action is needed.
                    }

                    // Wait exceed time if necessary
                    int waitTime = GetSecondsUntilEpoch(exceedResetEpoch);
                    if (waitTime > 0)
                        await Task.Delay(waitTime * 1000).ConfigureAwait(false);

                    // Let request continue
                    evt.Set();

                    // Reset new event for next request
                    newResetTimeEvent.Reset();
                }

                // Begin waiting again if the queue is empty
                if (queue.Count == 0)
                    queueAddEvent.Reset();
            }
        }

        public override void Dispose()
        {
            // Stop process task
            isProcessing = false;
            queueAddEvent.Set();
        }
    }
}
