namespace Discore.Http
{
    /// <summary>
    /// An optional set of parameters for modifying a guild.
    /// </summary>
    public class ModifyGuildOptions
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
        /// <para>Set to <see cref="Snowflake.None"/> to remove the AFK channel.</para>
        /// </summary>
        public Snowflake? AfkChannelId { get; set; }
        /// <summary>
        /// Gets or sets the time (in seconds) a member must be idle before being moved to the AFK channel 
        /// (or null to leave unchanged).
        /// </summary>
        public int? AfkTimeout { get; set; }
        /// <summary>
        /// Gets or sets the icon for the guild (or null to leave unchanged).
        /// <para>Set to <see cref="DiscordImageData.None"/> to remove the icon.</para>
        /// </summary>
        public DiscordImageData Icon { get; set; }
        /// <summary>
        /// Gets or sets the ID of the user to transfer guild ownership to (or null to leave unchanged) 
        /// (the current bot must be guild owner).
        /// </summary>
        public Snowflake? OwnerId { get; set; }
        /// <summary>
        /// Gets or sets the image splash for the guild (or null to leave unchanged) (VIP guilds only).
        /// <para>Set to <see cref="DiscordImageData.None"/> to remove the splash.</para>
        /// </summary>
        public DiscordImageData Splash { get; set; }

        /// <summary>
        /// Sets the name of the guild.
        /// </summary>
        public ModifyGuildOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the voice region the guild will use.
        /// </summary>
        public ModifyGuildOptions SetVoiceRegion(string voiceRegion)
        {
            VoiceRegion = voiceRegion;
            return this;
        }

        /// <summary>
        /// Sets the required verification level for the guild.
        /// </summary>
        public ModifyGuildOptions SetVerificationLevel(int verificationLevel)
        {
            VerificationLevel = verificationLevel;
            return this;
        }

        /// <summary>
        /// Sets the default message notification setting to be used by new members entering the guild.
        /// </summary>
        public ModifyGuildOptions SetDefaultMessageNotifications(int defualtMessageNotifications)
        {
            DefaultMessageNotifications = defualtMessageNotifications;
            return this;
        }

        /// <summary>
        /// Sets the ID of the AFK channel for the guild.
        /// </summary>
        /// <param name="afkChannelId">The ID of the AFK channel or <see cref="Snowflake.None"/> to remove the AFK channel.</param>
        public ModifyGuildOptions SetAfkChannel(Snowflake afkChannelId)
        {
            AfkChannelId = afkChannelId;
            return this;
        }

        /// <summary>
        /// Sets the time (in seconds) a member must be idle before being moved to the AFK channel.
        /// </summary>
        public ModifyGuildOptions SetAfkTimeout(int afkTimeout)
        {
            AfkTimeout = afkTimeout;
            return this;
        }

        /// <summary>
        /// Sets the guild icon.
        /// </summary>
        /// <param name="icon">The avatar data or <see cref="DiscordImageData.None"/> to remove the icon.</param>
        public ModifyGuildOptions SetIcon(DiscordImageData icon)
        {
            Icon = icon;
            return this;
        }

        /// <summary>
        /// Sets the ID of the user to transfer guild ownership to (the current bot must be guild owner).
        /// </summary>
        public ModifyGuildOptions SetOwner(Snowflake ownerId)
        {
            OwnerId = ownerId;
            return this;
        }

        /// <summary>
        /// Sets the image splash for the guild (VIP guilds only). 
        /// </summary>
        /// <param name="splash">The avatar data or <see cref="DiscordImageData.None"/> to remove the splash.</param>
        public ModifyGuildOptions SetSplash(DiscordImageData splash)
        {
            Splash = splash;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            if (Name != null)
                data.Set("name", Name);
            if (VoiceRegion != null)
                data.Set("region", VoiceRegion);
            if (VerificationLevel.HasValue)
                data.Set("verification_level", VerificationLevel);
            if (DefaultMessageNotifications.HasValue)
                data.Set("default_message_notifications", DefaultMessageNotifications);
            if (AfkChannelId.HasValue)
                data.Set("afk_channel_id", AfkChannelId);
            if (AfkTimeout.HasValue)
                data.Set("afk_timeout", AfkTimeout);
            if (Icon != null)
                data.Set("icon", Icon.ToDataUriScheme());
            if (OwnerId.HasValue)
                data.Set("owner_id", OwnerId);
            if (Splash != null)
                data.Set("splash", Splash.ToDataUriScheme());

            return data;
        }
    }
}
