namespace Discore
{
    /// <summary>
    /// A brief version of a guild object.
    /// </summary>
    public class DiscordUserGuild : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of this user guild.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the name of this user guild.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the icon hash of this user guild.
        /// </summary>
        public string Icon { get; private set; }
        /// <summary>
        /// Gets whether or not the user is the owner of this user guild.
        /// </summary>
        public bool Owner { get; private set; }
        /// <summary>
        /// Gets the user's enabled/disabled permissions.
        /// </summary>
        public DiscordPermission Permissions { get; private set; }

        /// <summary>
        /// Updates this user guild with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this user guild with.</param>
        public void Update(DiscordApiData data)
        {
            Id          = data.GetString("id") ?? Id;
            Name        = data.GetString("name") ?? Name;
            Icon        = data.GetString("icon") ?? Icon;
            Owner       = data.GetBoolean("owner") ?? Owner;

            long? permissions = data.GetInt64("permissions");
            if (permissions.HasValue)
                Permissions = (DiscordPermission)permissions.Value;
        }

        #region Object Overrides
        /// <summary>
        /// Determines whether the specified <see cref="DiscordUserGuild"/> is equal 
        /// to the current user guild.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordUserGuild"/> to check.</param>
        public bool Equals(DiscordUserGuild other)
        {
            return Id == other?.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current user guild.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordUserGuild other = obj as DiscordUserGuild;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this user guild.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns the name of this user guild.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

#pragma warning disable 1591
        public static bool operator ==(DiscordUserGuild a, DiscordUserGuild b)
        {
            return a?.Id == b?.Id;
        }

        public static bool operator !=(DiscordUserGuild a, DiscordUserGuild b)
        {
            return a?.Id != b?.Id;
        }
#pragma warning restore 1591
        #endregion
    }
}
