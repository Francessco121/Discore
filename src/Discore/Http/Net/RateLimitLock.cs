using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class RateLimitLock
    {
        public bool RequiresWait => GetDelay() > 0;

        ulong resetAtEpochMilliseconds;
        AsyncLock mutex;

        public RateLimitLock()
        {
            mutex = new AsyncLock();
        }

        int GetDelay()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            ulong epochNowMilliseconds = (ulong)t.TotalMilliseconds;

            if (resetAtEpochMilliseconds <= epochNowMilliseconds)
                return 0;
            else
                return (int)(resetAtEpochMilliseconds - epochNowMilliseconds);
        }

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            int msDelay = GetDelay();
            return msDelay <= 0 ? Task.CompletedTask : Task.Delay(msDelay);
        }

        public void ResetAt(ulong epochSeconds)
        {
            resetAtEpochMilliseconds = epochSeconds * 1000;
        }

        public void ResetAfter(int milliseconds)
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            ulong epochNowMilliseconds = (ulong)t.TotalMilliseconds;

            resetAtEpochMilliseconds = epochNowMilliseconds + (ulong)milliseconds;
        }

        public AwaitableDisposable<IDisposable> LockAsync(CancellationToken cancellationToken)
        {
            return mutex.LockAsync(cancellationToken);
        }
    }
}
