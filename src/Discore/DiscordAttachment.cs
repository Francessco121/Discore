namespace Discore
{
    /// <summary>
    /// An attachment in a <see cref="DiscordMessage"/>.
    /// </summary>
    public class DiscordAttachment : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of the attachment.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the file name of the attachment.
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// Gets the byte file-size of the attachment.
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// Gets the url of this attachment.
        /// </summary>
        public string Url { get; private set; }
        /// <summary>
        /// Gets the proxy url of this attachment.
        /// </summary>
        public string ProxyUrl { get; private set; }
        /// <summary>
        /// Gets the pixel-width of this attachment.
        /// </summary>
        public int? Width { get; private set; }
        /// <summary>
        /// Gets the pixel-height of this attachment.
        /// </summary>
        public int? Height { get; private set; }

        /// <summary>
        /// Updates this attachment with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this attachment with.</param>
        public void Update(DiscordApiData data)
        {
            Id       = data.GetString("id") ?? Id;
            FileName = data.GetString("filename") ?? FileName;
            Size     = data.GetInteger("size") ?? Size;
            Url      = data.GetString("url") ?? Url;
            ProxyUrl = data.GetString("proxy_url") ?? ProxyUrl;
            Width    = data.GetInteger("width") ?? Width;
            Height   = data.GetInteger("height") ?? Height;
        }
    }
}
