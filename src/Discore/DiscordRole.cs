namespace Discore
{
    /// <summary>
    /// Roles represent a set of permissions attached to a group of users. Roles have unique names, 
    /// colors, and can be "pinned" to the side bar, causing their members to be listed separately. 
    /// Roles are unique per guild, and can have separate permission profiles for the global 
    /// context (guild) and channel context.
    /// </summary>
    public sealed class DiscordRole : DiscordIdObject
    {
        /// <summary>
        /// Gets the name of this role.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the displayed color of this role.
        /// </summary>
        public DiscordColor Color { get; }
        /// <summary>
        /// Gets whether this role is pinned in the user list of a guild.
        /// </summary>
        public bool IsHoisted { get; }
        /// <summary>
        /// Gets the ordering position of this role.
        /// </summary>
        public int Position { get; }
        /// <summary>
        /// Gets the permissions specified by this role.
        /// </summary>
        public DiscordPermission Permissions { get; }
        /// <summary>
        /// Gets whether this role is managed.
        /// </summary>
        public bool IsManaged { get; }
        /// <summary>
        /// Gets whether this role is mentionable.
        /// </summary>
        public bool IsMentionable { get; }

        internal DiscordRole(DiscordApiData data)
            : base(data)
        {
            Name = data.GetString("name");
            IsHoisted = data.GetBoolean("hoist").Value;
            Position = data.GetInteger("position").Value;
            IsManaged = data.GetBoolean("managed").Value;
            IsMentionable = data.GetBoolean("mentionable").Value;

            int color = data.GetInteger("color").Value;
            Color = DiscordColor.FromHexadecimal(color);

            long permissions = data.GetInt64("permissions").Value;
            Permissions = (DiscordPermission)permissions;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
