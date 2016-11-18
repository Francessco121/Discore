using System;
using System.Net.WebSockets;

namespace Discore.WebSocket.Net
{
    class DiscoreWebSocketException : DiscoreException
    {
        public WebSocketCloseStatus ErrorCode { get; }

        public DiscoreWebSocketException(string message, WebSocketCloseStatus errorCode)
            : base($"{message} ({(int)errorCode})")
        {
            ErrorCode = errorCode;
        }

        public DiscoreWebSocketException(string message, WebSocketCloseStatus errorCode, Exception innerException)
            : base($"{message} ({(int)errorCode})", innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
