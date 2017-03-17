using Discore.WebSocket;

namespace Discore.Voice
{
    public class MemberSpeakingEventArgs : VoiceConnectionEventArgs
    {
        public DiscordGuildMember Member { get; }
        public bool IsSpeaking { get; }

        internal MemberSpeakingEventArgs(DiscordGuildMember member, bool isSpeaking, Shard shard, 
            DiscordVoiceConnection connection) 
            : base(shard, connection)
        {
            Member = member;
            IsSpeaking = isSpeaking;
        }
    }
}
