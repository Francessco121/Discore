namespace Discore
{
    /// <summary>
    /// Roles represent a set of permissions attached to a group of users. Roles have unique names, 
    /// colors, and can be "pinned" to the side bar, causing their members to be listed separately. 
    /// Roles are unique per guild, and can have separate permission profiles for the global 
    /// context (guild) and channel context.
    /// </summary>
    public class DiscordRole : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of this role.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the name of this role.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the displayed color of this role.
        /// </summary>
        public DiscordColor Color { get; private set; }
        /// <summary>
        /// Gets whether or not this role is pinned in the user list of a <see cref="DiscordGuild"/>.
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
        /// Gets whether or not this role is managed.
        /// </summary>
        public bool IsManaged { get; private set; }
        /// <summary>
        /// Gets whether or not this role is mentionable.
        /// </summary>
        public bool IsMentionable { get; private set; }

        /// <summary>
        /// Updates this role with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this role with.</param>
        public void Update(DiscordApiData data)
        {
            Id              = data.GetString("id") ?? Id;
            Name            = data.GetString("name") ?? Name;
            IsHoisted       = data.GetBoolean("hoist") ?? IsHoisted;
            Position        = data.GetInteger("position") ?? Position;
            IsManaged       = data.GetBoolean("managed") ?? IsManaged;
            IsMentionable   = data.GetBoolean("mentionable") ?? IsMentionable;

            int? color = data.GetInteger("color");
            if (color.HasValue)
                Color = DiscordColor.FromHexadecimal(color.Value);

            long? permissions = data.GetInt64("permissions");
            if (permissions.HasValue)
                Permissions = (DiscordPermission)permissions.Value;
        }

        /// <summary>
        /// Gets the name of this role.
        /// </summary>
        /// <returns>Returns the name of this role.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
