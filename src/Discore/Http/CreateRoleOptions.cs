#nullable enable

using System.Text.Json;

namespace Discore.Http
{
    public class CreateRoleOptions
    {
        /// <summary>
        /// Gets or sets the name of the role to create.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Gets or sets the permissions of the role to create (or null to use default).
        /// </summary>
        public DiscordPermission? Permissions { get; set; }
        /// <summary>
        /// Gets or sets the color of the role to create (or null to use default).
        /// </summary>
        public DiscordColor? Color { get; set; }
        /// <summary>
        /// Gets or sets whether the created role should be displayed in the sidebar (or null to use default).
        /// </summary>
        public bool? IsHoisted { get; set; }
        /// <summary>
        /// Gets or sets whether the created role should be mentionable (or null to use default).
        /// </summary>
        public bool? IsMentionable { get; set; }

        public CreateRoleOptions() { }

        public CreateRoleOptions(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Sets the name of the role to create.
        /// </summary>
        public CreateRoleOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the permissions of the role to create.
        /// </summary>
        public CreateRoleOptions SetPermissions(DiscordPermission permissions)
        {
            Permissions = permissions;
            return this;
        }

        /// <summary>
        /// Sets the color of the role to create.
        /// </summary>
        public CreateRoleOptions SetColor(DiscordColor color)
        {
            Color = color;
            return this;
        }

        /// <summary>
        /// Sets whether the created role should be displayed in the sidebar.
        /// </summary>
        public CreateRoleOptions SetHoisted(bool hoist)
        {
            IsHoisted = hoist;
            return this;
        }

        /// <summary>
        /// Sets whether the created role should be mentionable.
        /// </summary>
        public CreateRoleOptions SetMentionable(bool mentionable)
        {
            IsMentionable = mentionable;
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteString("name", Name);

            if (Permissions.HasValue)
                writer.WriteNumber("permissions", (int)Permissions.Value);

            if (Color.HasValue)
                writer.WriteNumber("color", Color.Value.ToHexadecimal());

            if (IsHoisted.HasValue)
                writer.WriteBoolean("hoist", IsHoisted.Value);

            if (IsMentionable.HasValue)
                writer.WriteBoolean("mentionable", IsMentionable.Value);

            BuildAdditionalProperties(writer);

            writer.WriteEndObject();
        }

        protected virtual void BuildAdditionalProperties(Utf8JsonWriter writer) { }
    }
}

#nullable restore
