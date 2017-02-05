using System;
using System.Threading.Tasks;

namespace Discore.Http.Net
{
    abstract class RateLimitHandler : IDisposable
    {
        public abstract void ExceededRateLimit(RateLimitHeaders headers);
        public abstract Task UpdateValues(RateLimitHeaders headers);
        public abstract Task Wait();

        protected ulong GetEpochFromNow(int offsetSeconds)
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            ulong epochNowSeconds = (ulong)t.TotalSeconds;

            return epochNowSeconds + (ulong)offsetSeconds;
        }

        protected int GetSecondsUntilEpoch(ulong epochSeconds)
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            ulong epochNowSeconds = (ulong)t.TotalSeconds;

            return (int)(epochSeconds - epochNowSeconds);
        }

        public virtual void Dispose() { }
    }
}
