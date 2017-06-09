using System;

namespace Discore.WebSocket
{
    public class ShardEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the shard associated with the event.
        /// </summary>
        public Shard Shard { get; }

        internal ShardEventArgs(Shard shard)
        {
            Shard = shard;
        }
    }

    public class ShardFailureEventArgs : ShardEventArgs
    {
        /// <summary>
        /// Gets the reason as to why the shard failed.
        /// </summary>
        public ShardFailureReason Reason { get; }
        /// <summary>
        /// Gets a message describing the reason the shard failed.
        /// </summary>
        public string Message { get; }

        internal ShardFailureEventArgs(Shard shard, string message, ShardFailureReason reason)
            : base(shard)
        {
            Message = message;
            Reason = reason;
        }
    }
}
