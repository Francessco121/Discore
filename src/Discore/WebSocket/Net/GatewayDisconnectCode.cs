namespace Discore.WebSocket.Net
{
    enum GatewayDisconnectCode
    {
        UnknownError = 4000,
        UnknownOpCode,
        DecodeError,
        NotAuthenticated,
        AuthenticationFailed,
        AlreadyAuthenticated,
        InvalidSeq = 4007,
        RateLimited,
        SessionTimeout,
        InvalidShard
    }
}
