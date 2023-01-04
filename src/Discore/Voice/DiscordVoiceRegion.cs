using System.Text.Json;

namespace Discore.Voice
{
    /// <summary>
    /// A region for a voice server.
    /// </summary>
    public class DiscordVoiceRegion
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

        internal DiscordVoiceRegion(JsonElement json)
        {
            Id = json.GetProperty("id").GetString()!;
            Name = json.GetProperty("name").GetString()!;
            IsOptimal = json.GetProperty("optimal").GetBoolean();
            IsDeprecated = json.GetProperty("deprecated").GetBoolean();
            IsCustom = json.GetProperty("custom").GetBoolean();
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
