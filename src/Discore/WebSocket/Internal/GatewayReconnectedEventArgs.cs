using System;

namespace Discore.WebSocket.Internal
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
