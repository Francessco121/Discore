namespace Discore.WebSocket
{
    public enum ShardFailureReason
    {
        /// <summary>
        /// Should be reported to the Discore developers if received.
        /// </summary>
        Unknown,
        /// <summary>
        /// The shard was invalid, given the sharding settings for the application.
        /// </summary>
        ShardInvalid,
        /// <summary>
        /// The shard failed to authenticate with the Discord gateway websocket API.
        /// </summary>
        AuthenticationFailed
    }
}
