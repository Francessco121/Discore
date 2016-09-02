namespace Discore.Audio
{
    /// <summary>
    /// Used to represent a user's voice connection status.
    /// </summary>
    public class DiscordVoiceState : IDiscordObject
    {
        /// <summary>
        /// Gets the guild the user of this voice state is in.
        /// </summary>
        public DiscordGuild Guild { get; internal set; }
        /// <summary>
        /// Gets the voice channel this user is in.
        /// </summary>
        public DiscordGuildChannel Channel { get; internal set; }
        /// <summary>
        /// Gets the user this voice state is for.
        /// </summary>
        public DiscordUser User { get; internal set; }
        /// <summary>
        /// Gets the current session id of this voice state.
        /// </summary>
        public string SessionId { get; internal set; }
        /// <summary>
        /// Gets whether or not this user is server deaf.
        /// </summary>
        public bool IsServerDeaf { get; internal set; }
        /// <summary>
        /// Gets whether or not this user is server mute.
        /// </summary>
        public bool IsServerMute { get; internal set; }
        /// <summary>
        /// Gets whether or not this user has deafened themself.
        /// </summary>
        public bool IsSelfDeaf { get; internal set; }
        /// <summary>
        /// Gets whether or not this user has muted themself.
        /// </summary>
        public bool IsSelfMute { get; internal set; }
        /// <summary>
        /// Gets whether or not this user is muted by the active user connected to the API.
        /// </summary>
        public bool IsSuppressed { get; internal set; }

        DiscordApiCache cache;

        /// <summary>
        /// Creates a new <see cref="DiscordVoiceState"/> instance.
        /// </summary>
        /// <param name="client">The associated <see cref="IDiscordClient"/>.</param>
        public DiscordVoiceState(IDiscordClient client)
        {
            cache = client.Cache;
        }

        /// <summary>
        /// Updates this voice state with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this voice state with.</param>
        public void Update(DiscordApiData data)
        {
            SessionId = data.GetString("session_id") ?? SessionId;
            IsServerDeaf = data.GetBoolean("deaf") ?? IsServerDeaf;
            IsServerMute = data.GetBoolean("mute") ?? IsServerMute;
            IsSelfDeaf = data.GetBoolean("self_deaf") ?? IsSelfDeaf;
            IsSelfMute = data.GetBoolean("self_mute") ?? IsSelfMute;
            IsSuppressed = data.GetBoolean("suppress") ?? IsSuppressed;

            string guildId = data.GetString("guild_id");
            string channelId = data.GetString("channel_id");
            string userId = data.GetString("user_id") ?? data.LocateString("user.id");

            if (guildId != null)
            {
                DiscordGuild guild;
                if (!cache.TryGet(guildId, out guild))
                    DiscordLogger.Default.LogWarning($"[VOICE_STATE.UPDATE] Failed to locate guild with id {guildId}");
                else
                    Guild = guild;
            }

            if (channelId != null)
            {
                DiscordChannel voiceChannel;
                if (!cache.TryGet(channelId, out voiceChannel))
                    DiscordLogger.Default.LogWarning($"[VOICE_STATE.UPDATE] Failed to locate voice channel with id {channelId}");
                else
                    Channel = (DiscordGuildChannel)voiceChannel;
            }
            else
                Channel = null;

            DiscordUser user;
            if (userId == null || !cache.TryGet(userId, out user))
                DiscordLogger.Default.LogWarning($"[VOICE_STATE.UPDATE] Failed to locate user with id {userId}");
            else
                User = user;
        }

        /// <summary>
        /// Updates this voice state based on details about a <see cref="DiscordGuildMember"/>.
        /// </summary>
        /// <param name="data">The data used to update the <see cref="DiscordGuildMember"/>.</param>
        public void UpdateFromGuildMemberUpdate(DiscordApiData data)
        {
            IsServerDeaf = data.GetBoolean("deaf") ?? IsServerDeaf;
            IsServerMute = data.GetBoolean("mute") ?? IsServerMute;
        }

        /// <summary>
        /// Serializes this voice state into a <see cref="DiscordApiData"/> object.
        /// </summary>
        /// <returns>Returns a new <see cref="DiscordApiData"/> object with the properties of this voice state.</returns>
        public DiscordApiData Serialize()
        {
            DiscordApiData data = new DiscordApiData();
            data.Set("guild_id", Guild != null ? Guild.Id : null);
            data.Set("channel_id", Channel != null ? Channel.Id : null);
            data.Set("user_id", User.Id);
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
