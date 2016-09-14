
namespace Discore.Net
{
    /// <summary>
    /// A disconnect code sent by the Discord gateway.
    /// </summary>
    public enum GatewayDisconnectCode : int
    {
        #region Standard WebSocket Disconnect Codes
        /// <summary>
        /// The connection has closed after the request was fulfilled.
        /// </summary>
        NormalClosure = 1000,
        /// <summary>
        /// Indicates an endpoint is being removed. Either the server or client will
        /// become unavailable.
        /// </summary>
        EndpointUnavailable = 1001,
        /// <summary>
        /// The client or server is terminating the connection because of a protocol
        ///  error.
        /// </summary>
        ProtocolError = 1002,
        /// <summary>
        /// The client or server is terminating the connection because it cannot accept
        /// the data type it received.
        /// </summary>
        InvalidMessageType = 1003,
        /// <summary>
        /// No error specified.
        /// </summary>
        Empty = 1005,
        /// <summary>
        /// The client or server is terminating the connection because it has received
        /// data inconsistent with the message type.
        /// </summary>
        InvalidPayloadData = 1007,
        /// <summary>
        /// The connection will be closed because an endpoint has received a message
        /// that violates its policy.
        /// </summary>
        PolicyViolation = 1008,
        /// <summary>
        /// Reserved for future use.
        /// </summary>
        MessageTooBig = 1009,
        /// <summary>
        /// The client is terminating the connection because it expected the server
        /// to negotiate an extension.
        /// </summary>
        MandatoryExtension = 1010,
        /// <summary>
        /// The connection will be closed by the server because of an error on the server.
        /// </summary>
        InternalServerError = 1011,
        #endregion

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
