namespace Discore.Audio
{
    public class DiscordVoiceState : IDiscordObject
    {
        public DiscordGuild Guild { get; internal set; }
        public DiscordGuildChannel Channel { get; internal set; }
        public DiscordUser User { get; internal set; }
        public string SessionId { get; internal set; }
        public bool IsServerDeaf { get; internal set; }
        public bool IsServerMute { get; internal set; }
        public bool IsSelfDeaf { get; internal set; }
        public bool IsSelfMute { get; internal set; }
        public bool IsSuppressed { get; internal set; }

        DiscordApiCache cache;

        public DiscordVoiceState(IDiscordClient client)
        {
            cache = client.Cache;
        }

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
