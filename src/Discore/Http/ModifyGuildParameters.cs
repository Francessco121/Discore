namespace Discore.Http
{
    /// <summary>
    /// Optional set of parameters for modifying a guild.
    /// </summary>
    public class ModifyGuildParameters
    {
        /// <summary>
        /// The guild name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The voice region id.
        /// </summary>
        public string VoiceRegion { get; set; }
        /// <summary>
        /// The required verification level.
        /// </summary>
        public int? VerificationLevel { get; set; }
        /// <summary>
        /// The default message notifications setting.
        /// </summary>
        public int? DefaultMessageNotifications { get; set; }
        /// <summary>
        /// The id of the AFK voice channel.
        /// </summary>
        public Snowflake? AfkChannelId { get; set; }
        /// <summary>
        /// The AFK time in seconds for users to be moved to the AFK channel.
        /// </summary>
        public int? AfkTimeout { get; set; }
        /// <summary>
        /// Base64 128x128 jpeg image for the guild icon.
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// User ID, used to transfer guild ownership (current authenticated user must be guild owner).
        /// </summary>
        public Snowflake? OwnerId { get; set; }
        /// <summary>
        /// Base64 128x128 jpeg image for the guild splash (VIP guilds only).
        /// </summary>
        public string Splash { get; set; }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("name", Name);
            data.Set("region", VoiceRegion);
            data.Set("verification_level", VerificationLevel);
            data.Set("default_message_notifications", DefaultMessageNotifications);
            data.Set("afk_channel_id", AfkChannelId);
            data.Set("afk_timeout", AfkTimeout);
            data.Set("icon", Icon);
            data.Set("owner_id", OwnerId);
            data.Set("splash", Splash);

            return data;
        }
    }
}
