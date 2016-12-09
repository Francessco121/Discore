namespace Discore
{
    public sealed class DiscordAttachment : DiscordIdObject
    {
        /// <summary>
        /// Gets the file name of the attachment.
        /// </summary>
        public string FileName { get; }
        /// <summary>
        /// Gets the byte file-size of the attachment.
        /// </summary>
        public int Size { get; }
        /// <summary>
        /// Gets the url of this attachment.
        /// </summary>
        public string Url { get; }
        /// <summary>
        /// Gets the proxy url of this attachment.
        /// </summary>
        public string ProxyUrl { get; }
        /// <summary>
        /// Gets the pixel-width of this attachment.
        /// </summary>
        public int? Width { get; }
        /// <summary>
        /// Gets the pixel-height of this attachment.
        /// </summary>
        public int? Height { get; }

        internal DiscordAttachment(DiscordApiData data)
            : base(data)
        {
            FileName = data.GetString("filename");
            Size     = data.GetInteger("size").Value;
            Url      = data.GetString("url");
            ProxyUrl = data.GetString("proxy_url");
            Width    = data.GetInteger("width");
            Height   = data.GetInteger("height");
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
