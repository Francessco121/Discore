namespace Discore
{
    /// <summary>
    /// Represents an Offline Guild, or a Guild whose information has not been provided 
    /// through Guild Create events during the Gateway connect.
    /// </summary>
    public class DiscordUnavailableGuild : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of the guild.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets whether or not this guild is unavailable.
        /// </summary>
        public bool Unavailable { get; private set; }

        /// <summary>
        /// Updates this unavailable guild with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this unavailable guild with.</param>
        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Unavailable = data.GetBoolean("unavailable") ?? Unavailable;
        }

        #region Object Overrides
        /// <summary>
        /// Determines whether the specified <see cref="DiscordUnavailableGuild"/> is equal 
        /// to the current unavailable guild.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordUnavailableGuild"/> to check.</param>
        public bool Equals(DiscordUnavailableGuild other)
        {
            return Id == other.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current unavailable guild.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordUnavailableGuild other = obj as DiscordUnavailableGuild;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this unavailable guild.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

#pragma warning disable 1591
        public static bool operator ==(DiscordUnavailableGuild a, DiscordUnavailableGuild b)
        {
            return a.Id == b.Id;
        }

        public static bool operator !=(DiscordUnavailableGuild a, DiscordUnavailableGuild b)
        {
            return a.Id != b.Id;
        }
#pragma warning restore 1591
        #endregion
    }
}
