using System;

namespace Discore.WebSocket.Net
{
    class GatewayHandshakeException : Exception
    {
        public GatewayFailureData FailureData { get; }

        public GatewayHandshakeException(GatewayFailureData failureData)
        {
            FailureData = failureData;
        }
    }
}
