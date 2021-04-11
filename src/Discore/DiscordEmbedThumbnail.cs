using System.Text.Json;

namespace Discore
{
    /// <summary>
    /// A thumbnail of a <see cref="DiscordEmbed"/>.
    /// </summary>
    public sealed class DiscordEmbedThumbnail
    {
        /// <summary>
        /// Gets the url of the thumbnail.
        /// </summary>
        public string? Url { get; }
        /// <summary>
        /// Gets the proxy url of the thumbnail.
        /// </summary>
        public string? ProxyUrl { get; }
        /// <summary>
        /// Gets the pixel-width of the thumbnail.
        /// </summary>
        public int? Width { get; }
        /// <summary>
        /// Gets the pixel-height of the thumbnail.
        /// </summary>
        public int? Height { get; }

        public DiscordEmbedThumbnail(string? url, string? proxyUrl, int? width, int? height)
        {
            Url = url;
            ProxyUrl = proxyUrl;
            Width = width;
            Height = height;
        }

        internal DiscordEmbedThumbnail(JsonElement json)
        {
            Url = json.GetPropertyOrNull("url")?.GetString();
            ProxyUrl = json.GetPropertyOrNull("proxy_url")?.GetString();
            Width = json.GetPropertyOrNull("width")?.GetInt32();
            Height = json.GetPropertyOrNull("height")?.GetInt32();
        }

        public override string ToString()
        {
            return Url ?? base.ToString();
        }
    }
}
