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
        public string Name { get; private set; }
        /// <summary>
        /// Gets the displayed color of this role.
        /// </summary>
        public DiscordColor Color { get; private set; }
        /// <summary>
        /// Gets whether this role is pinned in the user list of a guild.
        /// </summary>
        public bool IsHoisted { get; private set; }
        /// <summary>
        /// Gets the ordering position of this role.
        /// </summary>
        public int Position { get; private set; }
        /// <summary>
        /// Gets the permissions specified by this role.
        /// </summary>
        public DiscordPermission Permissions { get; private set; }
        /// <summary>
        /// Gets whether this role is managed.
        /// </summary>
        public bool IsManaged { get; private set; }
        /// <summary>
        /// Gets whether this role is mentionable.
        /// </summary>
        public bool IsMentionable { get; private set; }

        internal DiscordRole() { }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            Name = data.GetString("name") ?? Name;
            IsHoisted = data.GetBoolean("hoist") ?? IsHoisted;
            Position = data.GetInteger("position") ?? Position;
            IsManaged = data.GetBoolean("managed") ?? IsManaged;
            IsMentionable = data.GetBoolean("mentionable") ?? IsMentionable;

            int? color = data.GetInteger("color");
            if (color.HasValue)
                Color = DiscordColor.FromHexadecimal(color.Value);

            long? permissions = data.GetInt64("permissions");
            if (permissions.HasValue)
                Permissions = (DiscordPermission)permissions.Value;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
