using System;

namespace Discore.Voice
{
    /// <summary>
    /// Used to represent a user's voice connection status.
    /// </summary>
    public sealed class DiscordVoiceState
    {
        /// <summary>
        /// Gets the guild this voice state is for.
        /// </summary>
        public DiscordGuild Guild
        {
            get { return guildCache.Value; }
        }

        /// <summary>
        /// Gets the voice channel the user is in (or null if they are not in a voice channel).
        /// </summary>
        public DiscordGuildVoiceChannel Channel
        {
            get { return channelId.HasValue ? guildCache.VoiceChannels[channelId.Value]?.Value : null; }
        }

        /// <summary>
        /// Gets the user this voice state is for.
        /// </summary>
        public DiscordUser User
        {
            get { return cache.Users[userId]; }
        }

        /// <summary>
        /// Returns whether the user is in a voice channel.
        /// <para>Faster than checking if the Channel property is null as this avoids a cache hit.</para>
        /// </summary>
        public bool IsInVoiceChannel
        {
            get { return channelId.HasValue; }
        }

        /// <summary>
        /// Gets the ID of the guild this voice state is for.
        /// </summary>
        [Obsolete("Please use Guild.Id instead.")]
        public Snowflake? GuildId
        {
            get { return guildId; }
        }
        /// <summary>
        /// Gets the ID of the voice channel this user is in.
        /// </summary>
        [Obsolete("Please use Channel.Id instead.")]
        public Snowflake? ChannelId
        {
            get { return channelId; }
        }
        /// <summary>
        /// Gets the ID of the user this voice state is for.
        /// </summary>
        [Obsolete("Please use User.Id instead.")]
        public Snowflake UserId
        {
            get { return userId; }
        }
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

        DiscoreCache cache;
        DiscoreGuildCache guildCache;

        Snowflake guildId;
        Snowflake userId;
        Snowflake? channelId;

        internal DiscordVoiceState(DiscoreCache cache, DiscoreGuildCache guildCache, DiscordApiData data)
        {
            this.cache = cache;
            this.guildCache = guildCache;

            guildId = guildCache.DictionaryId;

            channelId    = data.GetSnowflake("channel_id");
            userId       = data.GetSnowflake("user_id").Value;

            SessionId    = data.GetString("session_id");
            IsServerDeaf = data.GetBoolean("deaf").Value;
            IsServerMute = data.GetBoolean("mute").Value;
            IsSelfDeaf   = data.GetBoolean("self_deaf").Value;
            IsSelfMute   = data.GetBoolean("self_mute").Value;
            IsSuppressed = data.GetBoolean("suppress").Value;
        }
    }
}
