using System.Text.Json;

namespace Discore
{
    public sealed class DiscordEmbedImage
    {
        /// <summary>
        /// Gets the source url of the image.
        /// </summary>
        public string? Url { get; }

        /// <summary>
        /// Gets a proxied url of the image.
        /// </summary>
        public string? ProxyUrl { get; }

        /// <summary>
        /// Gets the width of the image.
        /// </summary>
        public int? Width { get; }

        /// <summary>
        /// Gets the height of the image.
        /// </summary>
        public int? Height { get; }

        public DiscordEmbedImage(string? url, string? proxyUrl, int? width, int? height)
        {
            Url = url;
            ProxyUrl = proxyUrl;
            Width = width;
            Height = height;
        }

        internal DiscordEmbedImage(JsonElement json)
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
