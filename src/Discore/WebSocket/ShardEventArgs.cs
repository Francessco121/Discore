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

    public class ShardReconnectedEventArgs : ShardEventArgs
    {
        /// <summary>
        /// Gets whether the shard created a new session.
        /// <para/>
        /// A new session means that the bot's user presence may have been reset and any
        /// cached data is most likely invalid.
        /// When false, a resume was completed and any missed Gateway have already been
        /// received. Cached data may be kept when a resume occurs.
        /// </summary>
        public bool IsNewSession { get; }

        internal ShardReconnectedEventArgs(Shard shard, bool isNewSession) 
            : base(shard)
        {
            IsNewSession = isNewSession;
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
        /// <summary>
        /// If available, gets the exception that sparked the failure.
        /// </summary>
        public Exception? Exception { get; }

        internal ShardFailureEventArgs(Shard shard, string message, ShardFailureReason reason, Exception? ex)
            : base(shard)
        {
            Message = message;
            Reason = reason;
            Exception = ex;
        }
    }
}
