using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    class GatewayRateLimiter
    {
        public int ResetTimeSeconds => resetTime.Seconds;

        readonly TimeSpan resetTime;
        readonly int maxInvokes;

        DateTime resetAt;
        int invokesLeft;

        public GatewayRateLimiter(int resetTimeSeconds, int maxInvokes)
        {
            this.maxInvokes = maxInvokes;

            resetTime = TimeSpan.FromSeconds(resetTimeSeconds);

            Reset();
        }

        void Reset()
        {
            resetAt = DateTime.Now + resetTime;
            invokesLeft = maxInvokes;
        }

        /// <summary>
        /// Counts for one invocation of whatever this rate limiter represents.
        /// Will block the current thread until the specified time passes if there has been too many invocations.
        /// </summary>
        /// <exception cref="OperationCanceledException"></exception>
        public async Task Invoke(CancellationToken? cancellationToken = null)
        {
            if (invokesLeft > 0)
                invokesLeft--;
            else
            {
                CancellationToken ct = cancellationToken ?? CancellationToken.None;

                int waitTimeMs = (int)Math.Ceiling((resetAt - DateTime.Now).TotalMilliseconds);
                if (waitTimeMs > 0)
                    await Task.Delay(waitTimeMs, ct).ConfigureAwait(false);

                Reset();
                invokesLeft--;
            }
        }
    }
}
