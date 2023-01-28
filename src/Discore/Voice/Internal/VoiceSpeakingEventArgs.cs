using System;

namespace Discore.Voice.Internal
{
    class VoiceSpeakingEventArgs : EventArgs
    {
        public Snowflake UserId { get; }
        /// <summary>
        /// The user's SSRC.
        /// </summary>
        public int Ssrc { get; }
        public SpeakingFlag SpeakingFlag { get; }

        public VoiceSpeakingEventArgs(Snowflake userId, int ssrc, SpeakingFlag speakingFlag)
        {
            UserId = userId;
            Ssrc = ssrc;
            SpeakingFlag = speakingFlag;
        }
    }
}
