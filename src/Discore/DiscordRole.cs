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

        #region Object Overrides
        /// <summary>
        /// Determines whether the specified <see cref="DiscordRole"/> is equal 
        /// to the current role.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordRole"/> to check.</param>
        public bool Equals(DiscordRole other)
        {
            return Id == other?.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current role.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordRole other = obj as DiscordRole;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this role.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Gets the name of this role.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

#pragma warning disable 1591
        public static bool operator ==(DiscordRole a, DiscordRole b)
        {
            return a?.Id == b?.Id;
        }

        public static bool operator !=(DiscordRole a, DiscordRole b)
        {
            return a?.Id != b?.Id;
        }
#pragma warning restore 1591
        #endregion
    }
}
