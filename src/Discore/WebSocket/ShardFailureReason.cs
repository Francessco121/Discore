namespace Discore.WebSocket
{
    public enum ShardFailureReason
    {
        /// <summary>
        /// Should be reported to the Discore developers if received.
        /// </summary>
        Unknown,
        /// <summary>
        /// An IO error occured while the shard was attempting to connect.
        /// This is most likely due to permission issues when Discore attempts to
        /// access discore-local-storage.json.
        /// </summary>
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
