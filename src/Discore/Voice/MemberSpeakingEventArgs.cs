using Discore.WebSocket;

namespace Discore.Voice
{
    public class MemberSpeakingEventArgs : VoiceConnectionEventArgs
    {
        public Snowflake GuildId { get; }
        public Snowflake UserId { get; }
        public bool IsSpeaking { get; }

        internal MemberSpeakingEventArgs(Snowflake guildId, Snowflake userId, bool isSpeaking, Shard shard, 
            DiscordVoiceConnection connection) 
            : base(shard, connection)
        {
            GuildId = guildId;
            UserId = userId;
            IsSpeaking = isSpeaking;
        }
    }
}
