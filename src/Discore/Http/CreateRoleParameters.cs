namespace Discore.Http
{
    public class CreateRoleParameters
    {
        /// <summary>
        /// Gets or sets the name of the role to create.
        /// </summary>
        public string Name { get; set; }
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

        public CreateRoleParameters() { }

        public CreateRoleParameters(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Sets the name of the role to create.
        /// </summary>
        public CreateRoleParameters SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the permissions of the role to create.
        /// </summary>
        public CreateRoleParameters SetPermissions(DiscordPermission permissions)
        {
            Permissions = permissions;
            return this;
        }

        /// <summary>
        /// Sets the color of the role to create.
        /// </summary>
        public CreateRoleParameters SetColor(DiscordColor color)
        {
            Color = color;
            return this;
        }

        /// <summary>
        /// Sets whether the created role should be displayed in the sidebar.
        /// </summary>
        public CreateRoleParameters SetHoisted(bool hoist)
        {
            IsHoisted = hoist;
            return this;
        }

        /// <summary>
        /// Sets whether the created role should be mentionable.
        /// </summary>
        public CreateRoleParameters SetMentionable(bool mentionable)
        {
            IsMentionable = mentionable;
            return this;
        }

        internal virtual DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData();
            data.Set("name", Name);

            if (Permissions.HasValue)
                data.Set("permissions", (int)Permissions.Value);

            if (Color.HasValue)
                data.Set("color", Color.Value.ToHexadecimal());

            if (IsHoisted.HasValue)
                data.Set("hoist", IsHoisted);

            if (IsMentionable.HasValue)
                data.Set("mentionable", IsMentionable);

            return data;
        }
    }
}
