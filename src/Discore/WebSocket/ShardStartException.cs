namespace Discore.WebSocket
{
    public class ShardStartException : DiscoreException
    {
        public Shard Shard { get; }
        public ShardStartFailReason Reason { get; }

        public ShardStartException(string message, Shard shard, ShardStartFailReason reason)
            : base(message)
        {
            Shard = shard;
            Reason = reason;
        }
    }
}
