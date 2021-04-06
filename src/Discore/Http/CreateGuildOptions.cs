using System.Collections.Generic;
using System.Text.Json;

#nullable enable

namespace Discore.Http
{
    /// <summary>
    /// A set of options to use when creating a new guild.
    /// </summary>
    public class CreateGuildOptions
    {
        /// <summary>
        /// Gets or sets the name of the guild.
        /// <para>Note: cannot be null.</para>
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the ID of the voice region the guild should use (or null to use default).
        /// </summary>
        public string? VoiceRegion { get; set; }

        /// <summary>
        /// Gets or sets the icon of the guild (or null to use default).
        /// </summary>
        public DiscordImageData? Icon { get; set; }

        // TODO: Why isnt this GuildVerificationLevel
        /// <summary>
        /// Gets or sets the verification level of the guild (or null to use default).
        /// </summary>
        public int? VerificationLevel { get; set; }

        // TODO: Why isnt this GuildNotificationOption
        /// <summary>
        /// Gets or sets the default notification level for new members joining the guild (or null to use default).
        /// </summary>
        public int? DefaultMessageNotificationsLevel { get; set; }

        /// <summary>
        /// Gets or sets the initial roles in the guild (or null to not include any additional roles).
        /// <para>Note: The first role in this list will end up as the @everyone role.</para>
        /// </summary>
        public IList<CreateGuildRoleOptions>? Roles { get; set; }

        /// <summary>
        /// Gets or sets the initial text and voice channels in the guild (or null to use defaults).
        /// </summary>
        public IList<CreateGuildChannelOptions>? Channels { get; set; }

        /// <summary>
        /// Sets the name of the guild.
        /// </summary>
        public CreateGuildOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the ID of the voice region the guild should use.
        /// </summary>
        public CreateGuildOptions SetVoiceRegion(string voiceRegion)
        {
            VoiceRegion = voiceRegion;
            return this;
        }

        /// <summary>
        /// Sets the icon of the guild.
        /// </summary>
        public CreateGuildOptions SetIcon(DiscordImageData icon)
        {
            Icon = icon;
            return this;
        }

        /// <summary>
        /// Sets the verification level of the guild.
        /// </summary>
        public CreateGuildOptions SetVerificationLevel(int verificationLevel)
        {
            VerificationLevel = verificationLevel;
            return this;
        }

        /// <summary>
        /// Sets the default notification level for new members joining the guild.
        /// </summary>
        public CreateGuildOptions SetDefaultMessageNotificationsLevel(int defaultMessageNotificationsLevel)
        {
            DefaultMessageNotificationsLevel = defaultMessageNotificationsLevel;
            return this;
        }

        /// <summary>
        /// Adds a role to be created with the guild.
        /// <para>Note: The first role added will end up as the @everyone role.</para>
        /// </summary>
        /// <param name="role">Note: If this is the first role, it will end up as the @everyone role.</param>
        public CreateGuildOptions AddRole(CreateGuildRoleOptions role)
        {
            if (Roles == null)
                Roles = new List<CreateGuildRoleOptions>();

            Roles.Add(role);
            return this;
        }

        /// <summary>
        /// Adds a text or voice channel to be created with the guild.
        /// </summary>
        public CreateGuildOptions AddChannel(CreateGuildChannelOptions channel)
        {
            if (Channels == null)
                Channels = new List<CreateGuildChannelOptions>();

            Channels.Add(channel);
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteString("name", Name);

            if (VoiceRegion != null)
                writer.WriteString("region", VoiceRegion);

            if (Icon != null)
                writer.WriteString("icon", Icon.ToDataUriScheme());

            if (VerificationLevel.HasValue)
                writer.WriteNumber("verification_level", VerificationLevel.Value);

            if (DefaultMessageNotificationsLevel.HasValue)
                writer.WriteNumber("default_message_notifications", DefaultMessageNotificationsLevel.Value);

            if (Roles != null)
            {
                writer.WriteStartArray("roles");

                foreach (CreateGuildRoleOptions roleParams in Roles)
                    roleParams.Build(writer);

                writer.WriteEndArray();
            }

            if (Channels != null)
            {
                writer.WriteStartArray("channels");

                foreach (CreateGuildChannelOptions channelParams in Channels)
                    channelParams.Build(writer);

                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}

#nullable restore
