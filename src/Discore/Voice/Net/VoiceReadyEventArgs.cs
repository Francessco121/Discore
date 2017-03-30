using System;

namespace Discore.Voice.Net
{
    class VoiceReadyEventArgs : EventArgs
    {
        public int Port { get; }
        public int Ssrc { get; }
        public string[] EncryptionModes { get; }

        public VoiceReadyEventArgs(int port, int ssrc, string[] encryptionModes)
        {
            Port = port;
            Ssrc = ssrc;
            EncryptionModes = encryptionModes;
        }
    }
}
