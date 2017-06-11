using System;

namespace Discore.WebSocket
{
    class ShardCacheException : Exception
    {
        internal ShardCacheException(string message)
            : base(message)
        { }
    }
}
