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
}
