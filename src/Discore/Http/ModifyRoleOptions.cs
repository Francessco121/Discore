#nullable enable

using System.Text.Json;

namespace Discore.Http
{
    /// <summary>
    /// An optional set of parameters used for modifying a guild role.
    /// </summary>
    public class ModifyRoleOptions
    {
        /// <summary>
        /// Gets or sets the name of the role (or null to leave unchanged).
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Gets or sets the permissions granted by this role (or null to leave unchanged).
        /// </summary>
        public DiscordPermission? Permissions { get; set; }
        /// <summary>
        /// Gets or sets the display color of the role (or null to leave unchanged).
        /// </summary>
        public DiscordColor? Color { get; set; }
        /// <summary>
        /// Gets or sets whether the role is displayed in the sidebar (or null to leave unchanged).
        /// </summary>
        public bool? IsHoisted { get; set; }
        /// <summary>
        /// Gets or sets whether the role is mentionable (or null to leave unchanged).
        /// </summary>
        public bool? IsMentionable { get; set; }

        /// <summary>
        /// Sets the name of the role.
        /// </summary>
        public ModifyRoleOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the permissions granted by the role.
        /// </summary>
        public ModifyRoleOptions SetPermissions(DiscordPermission permissions)
        {
            Permissions = permissions;
            return this;
        }

        /// <summary>
        /// Sets the display color of the role.
        /// </summary>
        public ModifyRoleOptions SetColor(DiscordColor color)
        {
            Color = color;
            return this;
        }

        /// <summary>
        /// Sets whether the role should be displayed in the sidebar.
        /// </summary>
        public ModifyRoleOptions SetHoisted(bool isHoisted)
        {
            IsHoisted = isHoisted;
            return this;
        }

        /// <summary>
        /// Sets whether the role is mentionable.
        /// </summary>
        public ModifyRoleOptions SetMentionable(bool isMentionable)
        {
            IsMentionable = isMentionable;
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            if (Name != null)
                writer.WriteString("name", Name);
            if (Permissions.HasValue)
                writer.WriteNumber("permissions", (long)Permissions.Value);
            if (Color.HasValue)
                writer.WriteNumber("color", Color.Value.ToHexadecimal());
            if (IsHoisted.HasValue)
                writer.WriteBoolean("hoist", IsHoisted.Value);
            if (IsMentionable.HasValue)
                writer.WriteBoolean("mentionable", IsMentionable.Value);

            writer.WriteEndObject();
        }
    }
}

#nullable restore
