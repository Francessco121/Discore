using System;

namespace Discore.WebSocket.Net
{
    class GatewayReconnectedEventArgs : EventArgs
    {
        public bool IsNewSession { get; }

        public GatewayReconnectedEventArgs(bool isNewSession)
        {
            IsNewSession = isNewSession;
        }
    }
}
