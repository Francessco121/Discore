using System;
using System.Net;

namespace Discore.Voice.Internal
{
    class VoiceReadyEventArgs : EventArgs
    {
        public IPAddress IP { get; }
        public int Port { get; }
        public uint Ssrc { get; }
        public string[] EncryptionModes { get; }

        public VoiceReadyEventArgs(IPAddress ip, int port, uint ssrc, string[] encryptionModes)
        {
            IP = ip;
            Port = port;
            Ssrc = ssrc;
            EncryptionModes = encryptionModes;
        }
    }
}
