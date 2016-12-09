namespace Discore
{
    public sealed class DiscordEmbedVideo : IDiscordSerializable
    {
        /// <summary>
        /// Gets the source url of the video.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets the width of the video.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Gets the height of the video.
        /// </summary>
        public int Height { get; }

        internal DiscordEmbedVideo(DiscordApiData data)
        {
            Url = data.GetString("url");
            Width = data.GetInteger("width").Value;
            Height = data.GetInteger("height").Value;
        }

        public DiscordApiData Serialize()
        {
            DiscordApiData data = DiscordApiData.CreateContainer();
            data.Set("url", Url);
            data.Set("width", Width);
            data.Set("height", Height);
            return data;
        }

        public override string ToString()
        {
            return Url;
        }
    }
}
