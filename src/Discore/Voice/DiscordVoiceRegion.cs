using System.Text.Json;

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
        // TODO: looks like sample hostname/port were removed
        /// <summary>
        /// Gets an example hostname for the region.
        /// </summary>
        public string? SampleHostname { get; }
        /// <summary>
        /// Gets an example port for the region.
        /// </summary>
        public int? SamplePort { get; }
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

        public DiscordVoiceRegion(
            string id, 
            string name, 
            string? sampleHostname, 
            int? samplePort, 
            bool isVIPOnly, 
            bool isOptimal, 
            bool isDeprecated, 
            bool isCustom)
        {
            Id = id;
            Name = name;
            SampleHostname = sampleHostname;
            SamplePort = samplePort;
            IsVIPOnly = isVIPOnly;
            IsOptimal = isOptimal;
            IsDeprecated = isDeprecated;
            IsCustom = isCustom;
        }

        internal DiscordVoiceRegion(JsonElement json)
        {
            Id = json.GetProperty("id").GetString()!;
            Name = json.GetProperty("name").GetString()!;
            IsVIPOnly = json.GetProperty("vip").GetBoolean();
            IsOptimal = json.GetProperty("optimal").GetBoolean();
            IsDeprecated = json.GetProperty("deprecated").GetBoolean();
            IsCustom = json.GetProperty("custom").GetBoolean();

            SampleHostname = json.GetPropertyOrNull("sample_hostname")?.GetString();
            SamplePort = json.GetPropertyOrNull("sample_port")?.GetInt32();
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
