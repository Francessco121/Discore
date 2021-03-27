using System;

namespace Discore.WebSocket
{
    public enum ShardFailureReason
    {
        /// <summary>
        /// Should be reported to the Discore developers if received.
        /// </summary>
        Unknown,

        [Obsolete("This failure reason can no longer happen, any code handling it can be removed.")]
        IOError,
        /// <summary>
        /// The shard was invalid, given the sharding settings for the application.
        /// </summary>
        ShardInvalid,
        /// <summary>
        /// The shard failed to authenticate with the Discord Gateway WebSocket API.
        /// </summary>
        AuthenticationFailed,
        /// <summary>
        /// Occurs if only one shard is used, and that shard would have handled too many guilds.
        /// More than one shard is required if this happens.
        /// </summary>
        ShardingRequired
    }
}
