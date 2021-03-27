using System;

namespace Discore.WebSocket.Internal
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
