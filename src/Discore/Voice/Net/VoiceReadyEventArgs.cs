using System;
using System.Net;

namespace Discore.Voice.Net
{
    class VoiceReadyEventArgs : EventArgs
    {
        public IPAddress Ip { get; }
        public int Port { get; }
        public int Ssrc { get; }
        public string[] EncryptionModes { get; }

        public VoiceReadyEventArgs(IPAddress ip, int port, int ssrc, string[] encryptionModes)
        {
            Ip = ip;
            Port = port;
            Ssrc = ssrc;
            EncryptionModes = encryptionModes;
        }
    }
}
