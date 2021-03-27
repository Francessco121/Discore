namespace Discore.Voice
{
    /// <summary>
    /// A region for a voice server.
    /// </summary>
    public sealed class DiscordVoiceRegion
    {
        /// <summary>
        /// Gets the ID of the region.
        /// </summary>
        public string Id { get; }
        /// <summary>
        /// Gets the name of the region.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets an example hostname for the region.
        /// </summary>
        public string SampleHostname { get; }
        /// <summary>
        /// Gets an example port for the region.
        /// </summary>
        public int SamplePort { get; }
        /// <summary>
        /// Gets whether this is a vip-only server.
        /// </summary>
        public bool IsVIPOnly { get; }
        /// <summary>
        /// Gets whether this server is the closest to the user's client.
        /// </summary>
        public bool IsOptimal { get; }
        /// <summary>
        /// Gets whether this server is deprecated.
        /// </summary>
        public bool IsDeprecated { get; }
        /// <summary>
        /// Gets whether this is a custom voice region (used for events/etc.).
        /// </summary>
        public bool IsCustom { get; }

        internal DiscordVoiceRegion(DiscordApiData data)
        {
            Id             = data.GetString("id");
            Name           = data.GetString("name");
            SampleHostname = data.GetString("sample_hostname");
            SamplePort     = data.GetInteger("sample_port").Value;
            IsVIPOnly      = data.GetBoolean("vip").Value;
            IsOptimal      = data.GetBoolean("optimal").Value;
            IsDeprecated   = data.GetBoolean("deprecated").Value;
            IsCustom       = data.GetBoolean("custom").Value;
        }

        /// <summary>
        /// Returns the name of this voice region.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }
}
