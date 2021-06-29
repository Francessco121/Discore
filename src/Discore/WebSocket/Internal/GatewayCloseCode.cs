namespace Discore.WebSocket.Internal
{
    enum GatewayCloseCode
    {
        UnknownError = 4000,
        UnknownOpCode,
        DecodeError,
        NotAuthenticated,
        AuthenticationFailed,
        AlreadyAuthenticated,
        /// <summary>
        /// NOTE: this is not currently documented (as of gateway v5).
        /// </summary>
        InvalidSession = 4006,
        InvalidSeq = 4007,
        RateLimited,
        SessionTimeout,
        InvalidShard,
        ShardingRequired
    }
}
