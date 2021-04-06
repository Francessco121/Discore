using Nito.AsyncEx;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Discore.Http.Internal
{
    class RateLimitLock
    {
        /// <summary>
        /// Whether requests should be delayed in order to fulfill the rate-limit.
        /// </summary>
        public bool RequiresWait => MillisecondsUntilReset > 0;

        /// <summary>
        /// The number of milliseconds until the rate-limit is reset and requests can be made again.
        /// </summary>
        public double MillisecondsUntilReset => resetAtEpochMilliseconds - GetMillisecondsSinceEpoch();

        static readonly DateTime unixEpoch = new DateTime(1970, 1, 1);

        /// <summary>
        /// The number of seconds after the Unix Epoch when the rate-limit will reset.
        /// </summary>
        double resetAtEpochMilliseconds;

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
            double msDelay = MillisecondsUntilReset;

            return msDelay <= 0 
                ? Task.CompletedTask 
                : Task.Delay(TimeSpan.FromMilliseconds(msDelay), cancellationToken);
        }

        /// <summary>
        /// Sets the number of milliseconds after the Unix Epoch that must pass before this rate-limit resets.
        /// </summary>
        public void ResetAt(double millisecondsAfterUnixEpoch)
        {
            resetAtEpochMilliseconds = millisecondsAfterUnixEpoch;
        }

        /// <summary>
        /// Sets the number of milliseconds that must pass after the current time before this rate-limit resets.
        /// </summary>
        public void ResetAfter(double milliseconds)
        {
            resetAtEpochMilliseconds = GetMillisecondsSinceEpoch() + milliseconds;
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
            clone.resetAtEpochMilliseconds = resetAtEpochMilliseconds;

            return clone;
        }

        double GetMillisecondsSinceEpoch()
        {
            return (DateTime.UtcNow - unixEpoch).TotalMilliseconds;
        }
    }
}

#nullable restore
