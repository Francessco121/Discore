namespace Discore.Http
{
    /// <summary>
    /// An optional set of parameters for modifying a guild.
    /// </summary>
    public class ModifyGuildParameters
    {
        /// <summary>
        /// Gets or sets the guild name (or null to leave unchanged).
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the ID of the voice region the guild will use (or null to leave unchanged).
        /// </summary>
        public string VoiceRegion { get; set; }
        /// <summary>
        /// Gets or sets the required verification level (or null to leave unchanged).
        /// </summary>
        public int? VerificationLevel { get; set; }
        /// <summary>
        /// Gets or sets the default message notification setting to be used by new members entering the guild
        /// (or null to leave unchanged).
        /// </summary>
        public int? DefaultMessageNotifications { get; set; }
        /// <summary>
        /// Gets or sets the ID of the AFK voice channel (or null to leave unchanged).
        /// <para>Set to <see cref="Snowflake.Null"/> to remove the AFK channel.</para>
        /// </summary>
        public Snowflake? AfkChannelId { get; set; }
        /// <summary>
        /// Gets or sets the time (in seconds) a member must be idle before being moved to the AFK channel 
        /// (or null to leave unchanged).
        /// </summary>
        public int? AfkTimeout { get; set; }
        /// <summary>
        /// Gets or sets a base64 encoded 128x128 jpeg image for the guild icon (or null to leave unchanged).
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// Gets or sets the ID of the user to transfer guild ownership to (or null to leave unchanged) 
        /// (current authenticated user must be guild owner).
        /// </summary>
        public Snowflake? OwnerId { get; set; }
        /// <summary>
        /// Gets or sets a base64 encoded 128x128 jpeg image for the guild splash (or null to leave unchanged) (VIP guilds only).
        /// </summary>
        public string Splash { get; set; }

        /// <summary>
        /// Sets the name of the guild.
        /// </summary>
        public ModifyGuildParameters SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the voice region the guild will use.
        /// </summary>
        public ModifyGuildParameters SetVoiceRegion(string voiceRegion)
        {
            VoiceRegion = voiceRegion;
            return this;
        }

        /// <summary>
        /// Sets the required verification level for the guild.
        /// </summary>
        public ModifyGuildParameters SetVerificationLevel(int verificationLevel)
        {
            VerificationLevel = verificationLevel;
            return this;
        }

        /// <summary>
        /// Sets the default message notification setting to be used by new members entering the guild.
        /// </summary>
        public ModifyGuildParameters SetDefaultMessageNotifications(int defualtMessageNotifications)
        {
            DefaultMessageNotifications = defualtMessageNotifications;
            return this;
        }

        /// <summary>
        /// Sets the ID of the AFK channel for the guild.
        /// </summary>
        /// <param name="afkChannelId">The ID of the AFK channel or <see cref="Snowflake.Null"/> to remove the AFK channel.</param>
        public ModifyGuildParameters SetAfkChannel(Snowflake afkChannelId)
        {
            AfkChannelId = afkChannelId;
            return this;
        }

        /// <summary>
        /// Sets the time (in seconds) a member must be idle before being moved to the AFK channel.
        /// </summary>
        public ModifyGuildParameters SetAfkTimeout(int afkTimeout)
        {
            AfkTimeout = afkTimeout;
            return this;
        }

        /// <summary>
        /// Sets a base64 encoded 128x128 jpeg image for the guild icon.
        /// </summary>
        public ModifyGuildParameters SetIcon(string icon)
        {
            Icon = icon;
            return this;
        }

        /// <summary>
        /// Sets the ID of the user to transfer guild ownership to (current authenticated user must be guild owner).
        /// </summary>
        public ModifyGuildParameters SetOwner(Snowflake ownerId)
        {
            OwnerId = ownerId;
            return this;
        }

        /// <summary>
        /// Sets a base64 encoded 128x128 jpeg image for the guild splash (VIP guilds only). 
        /// </summary>
        public ModifyGuildParameters SetSplash(string splash)
        {
            Splash = splash;
            return this;
        }

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
