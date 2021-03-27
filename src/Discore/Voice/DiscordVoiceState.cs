namespace Discore.Voice
{
    /// <summary>
    /// Used to represent a user's voice connection status.
    /// </summary>
    public sealed class DiscordVoiceState
    {
        /// <summary>
        /// Gets the ID of the guild this voice state is for.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// Gets the ID of the voice channel the user is in (or null if they are not in a voice channel).
        /// </summary>
        public Snowflake? ChannelId { get; }
        /// <summary>
        /// Gets the ID of the user this voice state is for.
        /// </summary>
        public Snowflake UserId { get; }

        /// <summary>
        /// Gets the current session ID of this voice state.
        /// </summary>
        public string SessionId { get; }
        /// <summary>
        /// Gets whether or not this user is server deaf.
        /// </summary>
        public bool IsServerDeaf { get; }
        /// <summary>
        /// Gets whether or not this user is server mute.
        /// </summary>
        public bool IsServerMute { get; }
        /// <summary>
        /// Gets whether or not this user has deafened themself.
        /// </summary>
        public bool IsSelfDeaf { get; }
        /// <summary>
        /// Gets whether or not this user has muted themself.
        /// </summary>
        public bool IsSelfMute { get; }
        /// <summary>
        /// Gets whether or not this user is muted by the active user connected to the API.
        /// </summary>
        public bool IsSuppressed { get; }

        internal DiscordVoiceState(Snowflake guildId, Snowflake userId, Snowflake channelId)
        {
            GuildId = guildId;
            UserId = userId;
            ChannelId = channelId;
        }

        internal DiscordVoiceState(Snowflake guildId, DiscordApiData data)
        {
            GuildId      = guildId;

            ChannelId    = data.GetSnowflake("channel_id");
            UserId       = data.GetSnowflake("user_id").Value;

            SessionId    = data.GetString("session_id");
            IsServerDeaf = data.GetBoolean("deaf").Value;
            IsServerMute = data.GetBoolean("mute").Value;
            IsSelfDeaf   = data.GetBoolean("self_deaf").Value;
            IsSelfMute   = data.GetBoolean("self_mute").Value;
            IsSuppressed = data.GetBoolean("suppress").Value;
        }
    }
}
