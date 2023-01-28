namespace Discore.Voice
{
    public class MemberSpeakingEventArgs : VoiceConnectionEventArgs
    {
        /// <summary>
        /// Gets the ID of the guild that the user is speaking in.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// Gets the ID of the user who started/stopped speaking.
        /// </summary>
        public Snowflake UserId { get; }
        /// <summary>
        /// Gets the speaking state of the user.
        /// </summary>
        public SpeakingFlag SpeakingFlag { get; }
        /// <summary>
        /// Gets whether the user is currently speaking.
        /// </summary>
        public bool IsSpeaking => SpeakingFlag != SpeakingFlag.Off;

        internal MemberSpeakingEventArgs(Snowflake guildId, Snowflake userId, SpeakingFlag speakingFlag,
            DiscordVoiceConnection connection)
            : base(connection)
        {
            GuildId = guildId;
            UserId = userId;
            SpeakingFlag = speakingFlag;
        }
    }
}
