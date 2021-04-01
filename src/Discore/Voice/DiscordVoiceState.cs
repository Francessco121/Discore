#nullable enable

using System.Text.Json;

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
        public Snowflake? GuildId { get; }

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

        // TODO: add self_stream, self_video

        public DiscordVoiceState(
            Snowflake? guildId,
            Snowflake? channelId,
            Snowflake userId,
            string sessionId,
            bool isServerDeaf,
            bool isServerMute,
            bool isSelfDeaf,
            bool isSelfMute,
            bool isSuppressed)
        {
            GuildId = guildId;
            ChannelId = channelId;
            UserId = userId;
            SessionId = sessionId;
            IsServerDeaf = isServerDeaf;
            IsServerMute = isServerMute;
            IsSelfDeaf = isSelfDeaf;
            IsSelfMute = isSelfMute;
            IsSuppressed = isSuppressed;
        }

        internal DiscordVoiceState(Snowflake guildId, Snowflake userId, Snowflake channelId)
        {
            GuildId = guildId;
            UserId = userId;
            ChannelId = channelId;
            SessionId = "";
        }

        internal DiscordVoiceState(JsonElement json, Snowflake? guildId)
        {
            GuildId = guildId ?? json.GetPropertyOrNull("guild_id")?.GetSnowflake();

            ChannelId = json.GetProperty("channel_id").GetSnowflakeOrNull();
            UserId = json.GetProperty("user_id").GetSnowflake();

            SessionId = json.GetProperty("session_id").GetString()!;
            IsServerDeaf = json.GetProperty("deaf").GetBoolean();
            IsServerMute = json.GetProperty("mute").GetBoolean();
            IsSelfDeaf = json.GetProperty("self_deaf").GetBoolean();
            IsSelfMute = json.GetProperty("self_mute").GetBoolean();
            IsSuppressed = json.GetProperty("suppress").GetBoolean();
        }
    }
}

#nullable restore
