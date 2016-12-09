namespace Discore
{
    /// <summary>
    /// Used to represent a user's voice connection status.
    /// </summary>
    public class DiscordVoiceState : DiscordHashableObject
    {
        internal override Snowflake DictionaryId { get { return UserId; } }

        /// <summary>
        /// Gets the id of the guild this voice state is for.
        /// </summary>
        public Snowflake? GuildId { get; }
        /// <summary>
        /// Gets the id of the voice channel this user is in.
        /// </summary>
        public Snowflake? ChannelId { get; }
        /// <summary>
        /// Gets the id of the user this voice state is for.
        /// </summary>
        public Snowflake UserId { get; }
        /// <summary>
        /// Gets the current session id of this voice state.
        /// </summary>
        public string SessionId { get; }
        /// <summary>
        /// Gets whether or not this user is server deaf.
        /// </summary>
        public bool IsServerDeaf { get; private set; }
        /// <summary>
        /// Gets whether or not this user is server mute.
        /// </summary>
        public bool IsServerMute { get; private set; }
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

        internal DiscordVoiceState(DiscordApiData data)
        {
            GuildId      = data.GetSnowflake("guild_id");
            ChannelId    = data.GetSnowflake("channel_id");
            UserId       = data.GetSnowflake("user_id").Value;
            SessionId    = data.GetString("session_id");
            IsServerDeaf = data.GetBoolean("deaf").Value;
            IsServerMute = data.GetBoolean("mute").Value;
            IsSelfDeaf   = data.GetBoolean("self_deaf").Value;
            IsSelfMute   = data.GetBoolean("self_mute").Value;
            IsSuppressed = data.GetBoolean("suppress").Value;
        }

        internal DiscordVoiceState PartialUpdate(DiscordApiData data)
        {
            DiscordVoiceState newState = (DiscordVoiceState)MemberwiseClone();
            newState.IsServerDeaf = data.GetBoolean("deaf") ?? newState.IsServerDeaf;
            newState.IsServerMute = data.GetBoolean("mute") ?? newState.IsServerMute;

            return newState;
        }

        /// <summary>
        /// Serializes this voice state into a <see cref="DiscordApiData"/> object.
        /// </summary>
        /// <returns>Returns a new <see cref="DiscordApiData"/> object with the properties of this voice state.</returns>
        public DiscordApiData Serialize()
        {
            DiscordApiData data = new DiscordApiData();
            //data.Set("guild_id", Guild != null ? new Snowflake?(Guild.Id) : null);
            data.Set("channel_id", ChannelId);
            data.Set("user_id", UserId);
            data.Set("session_id", SessionId);
            data.Set("deaf", IsServerDeaf);
            data.Set("mute", IsServerMute);
            data.Set("self_deaf", IsSelfDeaf);
            data.Set("self_mute", IsSelfMute);
            data.Set("suppress", IsSuppressed);

            return data;
        }
    }
}
