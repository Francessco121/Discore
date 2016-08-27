namespace Discore.Audio
{
    /// <summary>
    /// A region for a voice server.
    /// </summary>
    public class DiscordVoiceRegion : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of the region.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the name of the region.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets an example hostname for the region.
        /// </summary>
        public string SampleHostname { get; private set; }
        /// <summary>
        /// Gets an example port for the region.
        /// </summary>
        public int SamplePort { get; private set; }
        /// <summary>
        /// Gets whether or not this is a vip-only server.
        /// </summary>
        public bool VIPOnly { get; private set; }
        /// <summary>
        /// Gets whether or not this server is the closest to the user's client.
        /// </summary>
        public bool Optimal { get; private set; }

        /// <summary>
        /// Updates this voice region with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this voice region with.</param>
        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Name = data.GetString("name") ?? Name;
            SampleHostname = data.GetString("sample_hostname") ?? SampleHostname;
            SamplePort = data.GetInteger("sample_port") ?? SamplePort;
            VIPOnly = data.GetBoolean("vip") ?? VIPOnly;
            Optimal = data.GetBoolean("optimal") ?? Optimal;
        }

        #region Object Overrides
        /// <summary>
        /// Determines whether the specified <see cref="DiscordVoiceRegion"/> is equal 
        /// to the current voice region.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordVoiceRegion"/> to check.</param>
        public bool Equals(DiscordVoiceRegion other)
        {
            return Id == other.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current voice region.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordVoiceRegion other = obj as DiscordVoiceRegion;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this voice region.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns the name of this voice region.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

#pragma warning disable 1591
        public static bool operator ==(DiscordVoiceRegion a, DiscordVoiceRegion b)
        {
            return a.Id == b.Id;
        }

        public static bool operator !=(DiscordVoiceRegion a, DiscordVoiceRegion b)
        {
            return a.Id != b.Id;
        }
#pragma warning restore 1591
        #endregion
    }
}
