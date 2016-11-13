namespace Discore
{
    /// <summary>
    /// A thumbnail of a <see cref="DiscordEmbed"/>.
    /// </summary>
    public sealed class DiscordEmbedThumbnail : DiscordObject
    {
        /// <summary>
        /// Gets the url of the thumbnail.
        /// </summary>
        public string Url { get; private set; }
        /// <summary>
        /// Gets the proxy url of the thumbnail.
        /// </summary>
        public string ProxyUrl { get; private set; }
        /// <summary>
        /// Gets the pixel-width of the thumbnail.
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Gets the pixel-height of the thumbnail.
        /// </summary>
        public int Height { get; private set; }

        internal DiscordEmbedThumbnail() { }

        internal override void Update(DiscordApiData data)
        {
            Url = data.GetString("url") ?? Url;
            ProxyUrl = data.GetString("proxy_url") ?? ProxyUrl;
            Width = data.GetInteger("width") ?? Width;
            Height = data.GetInteger("height") ?? Height;
        }

        public override string ToString()
        {
            return Url;
        }
    }
}
