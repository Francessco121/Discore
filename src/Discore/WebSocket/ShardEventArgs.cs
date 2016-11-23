using System;

namespace Discore.WebSocket
{
    public class ShardEventArgs : EventArgs
    {
        public Shard Shard { get; }

        internal ShardEventArgs(Shard shard)
        {
            Shard = shard;
        }
    }

    public class ShardFailureEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the shard that failed.
        /// </summary>
        public Shard Shard { get; }
        /// <summary>
        /// Gets the reason the shard failed.
        /// </summary>
        public ShardFailureReason Reason { get; }

        internal ShardFailureEventArgs(Shard shard, ShardFailureReason reason)
        {
            Shard = shard;
            Reason = reason;
        }
    }
}
