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
        /// A new session means that the bot's user status may have been reset,
        /// and the cache has been cleared.
        /// </summary>
        public bool IsNewSession { get; }

        public ShardReconnectedEventArgs(Shard shard, bool isNewSession) 
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
        public Exception Exception { get; }

        internal ShardFailureEventArgs(Shard shard, string message, ShardFailureReason reason, Exception ex)
            : base(shard)
        {
            Message = message;
            Reason = reason;
            Exception = ex;
        }
    }
}
