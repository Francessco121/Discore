using System;

namespace Discore.Voice.Internal
{
    class VoiceSessionDescriptionEventArgs : EventArgs
    {
        public byte[] SecretKey { get; }
        public string Mode { get; }

        public VoiceSessionDescriptionEventArgs(byte[] secretKey, string mode)
        {
            SecretKey = secretKey;
            Mode = mode;
        }
    }
}
