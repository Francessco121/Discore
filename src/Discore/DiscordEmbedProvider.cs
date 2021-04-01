#nullable enable

using System.Text.Json;

namespace Discore
{
    /// <summary>
    /// The web provider of a <see cref="DiscordEmbed"/>.
    /// </summary>
    public sealed class DiscordEmbedProvider
    {
        /// <summary>
        /// Gets the name of this provider.
        /// </summary>
        public string? Name { get; }
        /// <summary>
        /// Gets the url of this provider.
        /// </summary>
        public string? Url { get; }

        public DiscordEmbedProvider(string? name, string? url)
        {
            Name = name;
            Url = url;
        }

        internal DiscordEmbedProvider(JsonElement json)
        {
            Name = json.GetPropertyOrNull("name")?.GetString();
            Url = json.GetPropertyOrNull("url")?.GetString();
        }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }
}

#nullable restore
