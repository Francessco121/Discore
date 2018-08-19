using System;

namespace Discore.Voice.Internal
{
    class IPDiscoveryEventArgs : EventArgs
    {
        public string IP { get; }
        public int Port { get; }

        public IPDiscoveryEventArgs(string ip, int port)
        {
            IP = ip;
            Port = port;
        }
    }
}
