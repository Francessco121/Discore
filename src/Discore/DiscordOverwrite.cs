namespace Discore
{
    /// <summary>
    /// A permission overwrite for a <see cref="DiscordRole"/> or <see cref="DiscordGuildMember"/>.
    /// </summary>
    public class DiscordOverwrite : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of this permission overwrite.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// The type of this overwrite.
        /// </summary>
        public DiscordOverwriteType Type { get; private set; }
        /// <summary>
        /// The specifically allowed permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Allow { get; private set; }
        /// <summary>
        /// The specifically denied permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Deny { get; private set; }

        /// <summary>
        /// Updates this overwrite with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this overwrite with.</param>
        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;

            string type = data.GetString("type");
            if (type != null)
            {
                switch (type)
                {
                    case "role":
                        Type = DiscordOverwriteType.Role;
                        break;
                    case "member":
                        Type = DiscordOverwriteType.Member;
                        break;
                }
            }

            long? allow = data.GetInt64("allow");
            if (allow.HasValue)
                Allow = (DiscordPermission)allow.Value;

            long? deny = data.GetInt64("deny");
            if (deny.HasValue)
                Deny = (DiscordPermission)deny.Value;
        }

        #region Object Overrides
        /// <summary>
        /// Determines whether the specified <see cref="DiscordOverwrite"/> is equal 
        /// to the current overwrite.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordOverwrite"/> to check.</param>
        public bool Equals(DiscordOverwrite other)
        {
            return Id == other.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current overwrite.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordOverwrite other = obj as DiscordOverwrite;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this overwrite.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

#pragma warning disable 1591
        public static bool operator ==(DiscordOverwrite a, DiscordOverwrite b)
        {
            return a.Id == b.Id;
        }

        public static bool operator !=(DiscordOverwrite a, DiscordOverwrite b)
        {
            return a.Id != b.Id;
        }
#pragma warning restore 1591
        #endregion
    }
}
