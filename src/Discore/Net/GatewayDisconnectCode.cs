
namespace Discore.Net
{
    /// <summary>
    /// A disconnect code sent by the Discord gateway.
    /// </summary>
    public enum GatewayDisconnectCode : int
    {
        /// <summary>
        /// Unknown error occured, try reconnecting.
        /// </summary>
        UnknownError = 4000,
        /// <summary>
        /// Gateway received an invalid OP code.
        /// </summary>
        UnknownOpCode,
        /// <summary>
        /// Gateway failed to decode payload.
        /// </summary>
        DecodeError,
        /// <summary>
        /// Gateway received payload prior to identifying.
        /// </summary>
        NotAuthenticated,
        /// <summary>
        /// The token sent with the identify payload was incorrect.
        /// </summary>
        AuthenticationFailed,
        /// <summary>
        /// Gateway received more than one identify payload.
        /// </summary>
        AlreadyAuthenticated,
        /// <summary>
        /// The sequence sent when resuming the session was invalid.
        /// Reconnect and start a new session.
        /// </summary>
        InvalidSeq = 4007,
        /// <summary>
        /// Client is sending payloads too quickly.
        /// </summary>
        RateLimited,
        /// <summary>
        /// Client session timed out. Reconnect and start a new one.
        /// </summary>
        SessionTimeout,
        /// <summary>
        /// Gateway received an invalid shard when identifying.
        /// </summary>
        InvalidShard
    }
}
