using System;

namespace Discore.WebSocket.Net
{
    class GatewayFailureData
    {
        public string Message { get; }
        public ShardFailureReason Reason { get; }
        public Exception Exception { get; }

        public GatewayFailureData(string message, ShardFailureReason reason, Exception ex)
        {
            Message = message;
            Reason = reason;
            Exception = ex;
        }
    }
}
