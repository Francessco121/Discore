using System;

namespace Discore.WebSocket
{
    public class ShardStartException : Exception
    {
        /// <summary>
        /// Gets the shard that failed to start.
        /// </summary>
        public Shard Shard { get; }
        /// <summary>
        /// Gets the reason describing why the shard failed to start.
        /// </summary>
        public ShardFailureReason Reason { get; }

        internal ShardStartException(string message, Shard shard, ShardFailureReason reason)
            : base(message)
        {
            Shard = shard;
            Reason = reason;
        }

        internal ShardStartException(string message, Shard shard, ShardFailureReason reason, Exception innerException)
            : base(message, innerException)
        {
            Shard = shard;
            Reason = reason;
        }
    }
}
