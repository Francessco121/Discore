using System;

namespace Discore.WebSocket.Net
{
    class GatewayHandshakeException : Exception
    {
        public GatewayCloseCode CloseCode { get; }

        public GatewayHandshakeException(GatewayCloseCode closeCode)
        {
            CloseCode = closeCode;
        }
    }
}
