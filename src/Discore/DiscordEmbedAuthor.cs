using System.Text.Json;

namespace Discore
{
    public class DiscordEmbedAuthor
    {
        /// <summary>
        /// Gets the name of the author.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets the url to the author.
        /// </summary>
        public string? Url { get; }

        /// <summary>
        /// Gets the url of an icon of the author.
        /// </summary>
        public string? IconUrl { get; }

        /// <summary>
        /// Gets a proxied url to the icon of the author.
        /// </summary>
        public string? ProxyIconUrl { get; }

        internal DiscordEmbedAuthor(JsonElement json)
        {
            Name = json.GetPropertyOrNull("name")?.GetString();
            Url = json.GetPropertyOrNull("url")?.GetString();
            IconUrl = json.GetPropertyOrNull("icon_url")?.GetString();
            ProxyIconUrl = json.GetPropertyOrNull("proxy_icon_url")?.GetString();
        }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }
}
