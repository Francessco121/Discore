namespace Discore.WebSocket
{
    public sealed class DiscordEmbedImage : DiscordObject
    {
        /// <summary>
        /// Gets the source url of the image (only http(s)).
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Gets a proxied url of the image.
        /// </summary>
        public string ProxyUrl { get; private set; }

        /// <summary>
        /// Gets the width of the image.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height of the image.
        /// </summary>
        public int Height { get; private set; }

        internal DiscordEmbedImage() { }

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
