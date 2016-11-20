namespace Discore.WebSocket
{
    public sealed class DiscordEmbedVideo : DiscordObject
    {
        /// <summary>
        /// Gets the source url of the video.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Gets the width of the video.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height of the video.
        /// </summary>
        public int Height { get; private set; }

        internal DiscordEmbedVideo() { }

        internal override void Update(DiscordApiData data)
        {
            Url = data.GetString("url") ?? Url;
            Width = data.GetInteger("width") ?? Width;
            Height = data.GetInteger("height") ?? Height;
        }

        public override string ToString()
        {
            return Url;
        }
    }
}
