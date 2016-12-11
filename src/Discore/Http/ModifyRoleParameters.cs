namespace Discore.Http
{
    /// <summary>
    /// A set of optional parameters used for modifying a guild role.
    /// </summary>
    public class ModifyRoleParameters
    {
        /// <summary>
        /// The name of the role.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Permissions granted by this role.
        /// </summary>
        public DiscordPermission? Permissions { get; set; }
        /// <summary>
        /// The sorting position of the role.
        /// </summary>
        public int? Position { get; set; }
        /// <summary>
        /// The UI color of the role.
        /// </summary>
        public DiscordColor? Color { get; set; }
        /// <summary>
        /// Whether the role is displayed seperately in the sidebar.
        /// </summary>
        public bool? IsHoisted { get; set; }
        /// <summary>
        /// Whether the role is mentionable.
        /// </summary>
        public bool? IsMentionable { get; set; }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("name", Name);
            data.Set("permissions", (long?)Permissions);
            data.Set("position", Position);
            data.Set("color", Color?.ToHexadecimal());
            data.Set("hoist", IsHoisted);
            data.Set("mentionable", IsMentionable);

            return data;
        }
    }
}
