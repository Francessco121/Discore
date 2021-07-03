using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Http.Internal
{
    class RateLimitLock
    {
        /// <summary>
        /// Whether requests should be delayed in order to fulfill the rate-limit.
        /// </summary>
        public bool RequiresWait => SecondsUntilReset > 0;

        /// <summary>
        /// The number of seconds until the rate-limit is reset and requests can be made again.
        /// </summary>
        public double SecondsUntilReset => resetAtEpochSeconds - GetSecondsSinceEpoch();

        static readonly DateTime unixEpoch = new DateTime(1970, 1, 1);

        /// <summary>
        /// The number of seconds after the Unix Epoch when the rate-limit will reset.
        /// </summary>
        double resetAtEpochSeconds;

        readonly AsyncLock mutex;

        public RateLimitLock()
        {
            mutex = new AsyncLock();
        }

        /// <summary>
        /// Returns a task which will complete after this rate-limit has been reset.
        /// The task will complete immediately if the reset time has already been passed.
        /// </summary>
        public Task WaitAsync(CancellationToken cancellationToken)
        {
            double secondsDelay = SecondsUntilReset;

            return secondsDelay <= 0 
                ? Task.CompletedTask 
                : Task.Delay(TimeSpan.FromSeconds(secondsDelay), cancellationToken);
        }

        /// <summary>
        /// Sets the number of seconds after the Unix Epoch that must pass before this rate-limit resets.
        /// </summary>
        public void ResetAt(double secondsAfterUnixEpoch)
        {
            resetAtEpochSeconds = secondsAfterUnixEpoch;
        }

        /// <summary>
        /// Sets the number of seconds that must pass after the current time before this rate-limit resets.
        /// </summary>
        public void ResetAfter(double seconds)
        {
            resetAtEpochSeconds = GetSecondsSinceEpoch() + seconds;
        }

        /// <summary>
        /// Returns a disposable asynchronous lock for this rate-limit.
        /// </summary>
        public AwaitableDisposable<IDisposable> LockAsync(CancellationToken cancellationToken)
        {
            return mutex.LockAsync(cancellationToken);
        }

        /// <summary>
        /// Returns a copy of this lock with the same reset time.
        /// </summary>
        public RateLimitLock Clone()
        {
            var clone = new RateLimitLock();
            clone.resetAtEpochSeconds = resetAtEpochSeconds;

            return clone;
        }

        double GetSecondsSinceEpoch()
        {
            return (DateTime.UtcNow - unixEpoch).TotalSeconds;
        }
    }
}
