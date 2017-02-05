using System;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    class GatewayRateLimiter
    {
        public int ResetTimeMs { get { return resetTimeMs; } }

        readonly int resetTimeMs;
        readonly int maxInvokes;

        int resetAtMs;
        int invokesLeft;

        public GatewayRateLimiter(int resetTimeMs, int maxInvokes)
        {
            this.resetTimeMs = resetTimeMs;
            this.maxInvokes = maxInvokes;

            Reset();
        }

        void Reset()
        {
            resetAtMs = Environment.TickCount + resetTimeMs;
            invokesLeft = maxInvokes;
        }

        /// <summary>
        /// Counts for one invocation of whatever this rate limiter represents.
        /// Will block the current thread until the specified time passes if there has been too many invocations.
        /// </summary>
        public async Task Invoke()
        {
            if (invokesLeft > 0)
                invokesLeft--;
            else
            {
                while (!TimeHelper.HasTickCountHit(resetAtMs))
                    // Wait 100ms at a time until the rate limiter has reset.
                    await Task.Delay(100).ConfigureAwait(false);

                Reset();
            }
        }
    }
}
