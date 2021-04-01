using System;

#nullable enable

namespace Discore.WebSocket
{
    class ShardCacheException : Exception
    {
        internal ShardCacheException(string message)
            : base(message)
        { }
    }
}

#nullable restore
