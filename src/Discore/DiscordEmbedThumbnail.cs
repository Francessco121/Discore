namespace Discore
{
    /// <summary>
    /// A thumbnail of a <see cref="DiscordEmbed"/>.
    /// </summary>
    public class DiscordEmbedThumbnail : IDiscordObject
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

        /// <summary>
        /// Updates this embed thumbnail with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this embed thumbnail with.</param>
        public void Update(DiscordApiData data)
        {
            Url = data.GetString("url") ?? Url;
            ProxyUrl = data.GetString("proxy_url") ?? ProxyUrl;
            Width = data.GetInteger("width") ?? Width;
            Height = data.GetInteger("height") ?? Height;
        }
    }
}
