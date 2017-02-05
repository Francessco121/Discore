using System;
using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class MinimalRateLimitHandler : RateLimitHandler
    {
        ulong resetEpoch;

        public override void ExceededRateLimit(RateLimitHeaders headers)
        {
            resetEpoch = GetEpochFromNow((int)Math.Ceiling(headers.RetryAfter.Value / 1000f));
        }

        public override Task UpdateValues(RateLimitHeaders headers) { return Task.CompletedTask; }

        public override Task Wait()
        {
            int waitTime = GetSecondsUntilEpoch(resetEpoch);
            if (waitTime > 0)
                return Task.Delay(waitTime * 1000);
            else
                return Task.CompletedTask;
        }
    }
}
