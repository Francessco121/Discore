namespace Discore.Audio
{
    /// <summary>
    /// A region for a voice server.
    /// </summary>
    public class DiscordVoiceRegion : DiscordIdObject
    {
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

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            Name = data.GetString("name") ?? Name;
            SampleHostname = data.GetString("sample_hostname") ?? SampleHostname;
            SamplePort = data.GetInteger("sample_port") ?? SamplePort;
            VIPOnly = data.GetBoolean("vip") ?? VIPOnly;
            Optimal = data.GetBoolean("optimal") ?? Optimal;
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
