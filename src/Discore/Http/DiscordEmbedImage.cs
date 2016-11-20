namespace Discore.Http
{
    public class DiscordEmbedImage
    {
        /// <summary>
        /// Gets the source url of the image (only http(s)).
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets a proxied url of the image.
        /// </summary>
        public string ProxyUrl { get; }

        /// <summary>
        /// Gets the width of the image.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the image.
        /// </summary>
        public int Height { get; }

        public DiscordEmbedImage(DiscordApiData data)
        {
            Url = data.GetString("url");
            ProxyUrl = data.GetString("proxy_url");
            Width = data.GetInteger("width").Value;
            Height = data.GetInteger("height").Value;
        }

        public override string ToString()
        {
            return Url;
        }
    }
}
