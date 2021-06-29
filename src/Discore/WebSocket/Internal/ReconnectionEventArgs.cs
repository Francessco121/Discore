using System;

namespace Discore.WebSocket.Internal
{
    class ReconnectionEventArgs : EventArgs
    {
        public bool CreateNewSession { get; }
        public int ConnectionDelayMs { get; }

        public ReconnectionEventArgs(bool createNewSession, int connectionDelayMs = 0)
        {
            CreateNewSession = createNewSession;
            ConnectionDelayMs = connectionDelayMs;
        }
    }
}
