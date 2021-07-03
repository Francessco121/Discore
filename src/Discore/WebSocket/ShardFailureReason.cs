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
        /// The shard failed to authenticate with the Discord Gateway WebSocket API.
        /// </summary>
        AuthenticationFailed,
        /// <summary>
        /// Occurs if only one shard is used, and that shard would have handled too many guilds.
        /// More than one shard is required if this happens.
        /// </summary>
        ShardingRequired,
        /// <summary>
        /// A specified intent was invalid. You may have incorrectly calculated the bitwse value.
        /// </summary>
        InvalidIntents,
        /// <summary>
        /// A disallowed Gateway intent was specified. May happen if you specify an intent that
        /// you have not enabled or are not approved for (i.e. privileged intents).
        /// </summary>
        DisallowedIntents
    }
}
