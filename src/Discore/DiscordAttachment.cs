namespace Discore
{
    public sealed class DiscordAttachment : DiscordIdObject
    {
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

        internal DiscordAttachment() { }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            FileName = data.GetString("filename") ?? FileName;
            Size = data.GetInteger("size") ?? Size;
            Url = data.GetString("url") ?? Url;
            ProxyUrl = data.GetString("proxy_url") ?? ProxyUrl;
            Width = data.GetInteger("width") ?? Width;
            Height = data.GetInteger("height") ?? Height;
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
