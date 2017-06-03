using System.Collections.Generic;

namespace Discore.Http
{
    /// <summary>
    /// A set of options to use when creating a new guild.
    /// </summary>
    public class CreateGuildParameters
    {
        /// <summary>
        /// Gets or sets the name of the guild.
        /// <para>Note: cannot be null.</para>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the ID of the voice region the guild should use (or null to use default).
        /// </summary>
        public string VoiceRegion { get; set; }

        /// <summary>
        /// Gets or sets the icon of the guild (or null to use default).
        /// </summary>
        public DiscordImageData Icon { get; set; }

        /// <summary>
        /// Gets or sets the verification level of the guild (or null to use default).
        /// </summary>
        public int? VerificationLevel { get; set; }

        /// <summary>
        /// Gets or sets the default notification level for new members joining the guild (or null to use default).
        /// </summary>
        public int? DefaultMessageNotificationsLevel { get; set; }

        /// <summary>
        /// Gets or sets the initial roles in the guild (or null to not include any additional roles).
        /// <para>Note: The first role in this list will end up as the @everyone role.</para>
        /// </summary>
        public IList<CreateGuildRoleParameters> Roles { get; set; }

        /// <summary>
        /// Gets or sets the initial text and voice channels in the guild (or null to use defaults).
        /// </summary>
        public IList<CreateGuildChannelParameters> Channels { get; set; }

        /// <summary>
        /// Sets the name of the guild.
        /// </summary>
        public CreateGuildParameters SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the ID of the voice region the guild should use.
        /// </summary>
        public CreateGuildParameters SetVoiceRegion(string voiceRegion)
        {
            VoiceRegion = voiceRegion;
            return this;
        }

        /// <summary>
        /// Sets the icon of the guild.
        /// </summary>
        public CreateGuildParameters SetIcon(DiscordImageData icon)
        {
            Icon = icon;
            return this;
        }

        /// <summary>
        /// Sets the verification level of the guild.
        /// </summary>
        public CreateGuildParameters SetVerificationLevel(int verificationLevel)
        {
            VerificationLevel = verificationLevel;
            return this;
        }

        /// <summary>
        /// Sets the default notification level for new members joining the guild.
        /// </summary>
        public CreateGuildParameters SetDefaultMessageNotificationsLevel(int defaultMessageNotificationsLevel)
        {
            DefaultMessageNotificationsLevel = defaultMessageNotificationsLevel;
            return this;
        }

        /// <summary>
        /// Adds a role to be created with the guild.
        /// <para>Note: The first role added will end up as the @everyone role.</para>
        /// </summary>
        /// <param name="role">Note: If this is the first role, it will end up as the @everyone role.</param>
        public CreateGuildParameters AddRole(CreateGuildRoleParameters role)
        {
            if (Roles == null)
                Roles = new List<CreateGuildRoleParameters>();

            Roles.Add(role);
            return this;
        }

        /// <summary>
        /// Adds a text or voice channel to be created with the guild.
        /// </summary>
        public CreateGuildParameters AddChannel(CreateGuildChannelParameters channel)
        {
            if (Channels == null)
                Channels = new List<CreateGuildChannelParameters>();

            Channels.Add(channel);
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);

            data.Set("name", Name);

            if (VoiceRegion != null)
                data.Set("region", VoiceRegion);

            if (Icon != null)
                data.Set("icon", Icon.ToDataUriScheme());

            if (VerificationLevel.HasValue)
                data.Set("verification_level", VerificationLevel.Value);

            if (DefaultMessageNotificationsLevel.HasValue)
                data.Set("default_message_notifications", DefaultMessageNotificationsLevel.Value);

            if (Roles != null)
            {
                DiscordApiData rolesArray = new DiscordApiData(DiscordApiDataType.Array);
                foreach (CreateGuildRoleParameters roleParams in Roles)
                    rolesArray.Values.Add(roleParams.Build());

                data.Set("roles", rolesArray);
            }

            if (Channels != null)
            {
                DiscordApiData channelsArray = new DiscordApiData(DiscordApiDataType.Array);
                foreach (CreateGuildChannelParameters channelParams in Channels)
                    channelsArray.Values.Add(channelParams.Build());

                data.Set("channels", channelsArray);
            }

            return data;
        }
    }
}
