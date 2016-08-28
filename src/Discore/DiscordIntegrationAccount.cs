namespace Discore
{
    /// <summary>
    /// The account of a <see cref="DiscordIntegration"/>.
    /// </summary>
    public class DiscordIntegrationAccount : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of this account.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the name of this account.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Updates this integration account with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this integration account with.</param>
        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Name = data.GetString("name") ?? Name;
        }

        #region Object Overrides
        /// <summary>
        /// Determines whether the specified <see cref="DiscordIntegrationAccount"/> is equal 
        /// to the current integration account.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordIntegrationAccount"/> to check.</param>
        public bool Equals(DiscordIntegrationAccount other)
        {
            return Id == other?.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current integration account.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordIntegrationAccount other = obj as DiscordIntegrationAccount;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this integration account.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns the name of this integration account.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

#pragma warning disable 1591
        public static bool operator ==(DiscordIntegrationAccount a, DiscordIntegrationAccount b)
        {
            return a?.Id == b?.Id;
        }

        public static bool operator !=(DiscordIntegrationAccount a, DiscordIntegrationAccount b)
        {
            return a?.Id != b?.Id;
        }
#pragma warning restore 1591
        #endregion
    }
}
