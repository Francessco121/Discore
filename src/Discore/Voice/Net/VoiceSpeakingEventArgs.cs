using System;

namespace Discore.Voice.Net
{
    class VoiceSpeakingEventArgs : EventArgs
    {
        public Snowflake UserId { get; }
        /// <summary>
        /// The user's SSRC.
        /// </summary>
        public int Ssrc { get; }
        public bool IsSpeaking { get; }

        public VoiceSpeakingEventArgs(Snowflake userId, int ssrc, bool isSpeaking)
        {
            UserId = userId;
            Ssrc = ssrc;
            IsSpeaking = isSpeaking;
        }
    }
}
