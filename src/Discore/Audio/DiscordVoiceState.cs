namespace Discore.Audio
{
    /// <summary>
    /// Used to represent a user's voice connection status.
    /// </summary>
    public class DiscordVoiceState : DiscordObject
    {
        /// <summary>
        /// Gets the guild the user of this voice state is in.
        /// </summary>
        public DiscordGuild Guild { get; private set; }
        /// <summary>
        /// Gets the voice channel this user is in.
        /// </summary>
        public DiscordGuildVoiceChannel Channel { get; private set; }
        /// <summary>
        /// Gets the user this voice state is for.
        /// </summary>
        public DiscordUser User { get; private set; }
        /// <summary>
        /// Gets the member this voice state is for.
        /// </summary>
        public DiscordGuildMember Member { get; private set; }
        /// <summary>
        /// Gets the current session id of this voice state.
        /// </summary>
        public string SessionId { get; private set; }
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
        public bool IsSelfDeaf { get; private set; }
        /// <summary>
        /// Gets whether or not this user has muted themself.
        /// </summary>
        public bool IsSelfMute { get; private set; }
        /// <summary>
        /// Gets whether or not this user is muted by the active user connected to the API.
        /// </summary>
        public bool IsSuppressed { get; private set; }

        Shard shard;

        internal DiscordVoiceState(Shard shard, DiscordGuildMember member)
        {
            this.shard = shard;
            Member = member;
        }

        internal override void Update(DiscordApiData data)
        {
            SessionId = data.GetString("session_id") ?? SessionId;
            IsServerDeaf = data.GetBoolean("deaf") ?? IsServerDeaf;
            IsServerMute = data.GetBoolean("mute") ?? IsServerMute;
            IsSelfDeaf = data.GetBoolean("self_deaf") ?? IsSelfDeaf;
            IsSelfMute = data.GetBoolean("self_mute") ?? IsSelfMute;
            IsSuppressed = data.GetBoolean("suppress") ?? IsSuppressed;

            // Get user
            string userId = data.GetString("user_id");

            if (userId != null)
                User = shard.Users.Get(userId);
            else
            {
                DiscordApiData userData = data.Get("user");
                if (userData != null)
                {
                    userId = userData.GetString("id");
                    User = shard.Users.Edit(userId, () => new DiscordUser(), user => user.Update(userData));
                }
            }

            // Get guild (if in voice channel)
            string guildId = data.GetString("guild_id");

            if (guildId != null)
                Guild = shard.Guilds.Get(guildId);

            // Get channel (if in voice channel)
            string channelId = data.GetString("channel_id");

            if (channelId != null && Guild != null)
            {
                Channel = Guild.VoiceChannels.Get(channelId);
                Channel.Members.Add(User.Id);
            }
            else
                Channel = null;
        }

        internal void UpdateFromGuildMemberUpdate(DiscordApiData data)
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
