using System.Text.Json;

namespace Discore
{
    public class DiscordEmbedVideo
    {
        /// <summary>
        /// Gets the source url of the video.
        /// </summary>
        public string? Url { get; }

        /// <summary>
        /// Gets the width of the video.
        /// </summary>
        public int? Width { get; }

        /// <summary>
        /// Gets the height of the video.
        /// </summary>
        public int? Height { get; }

        // TODO: add proxy_url

        public DiscordEmbedVideo(string? url, int? width, int? height)
        {
            Url = url;
            Width = width;
            Height = height;
        }

        internal DiscordEmbedVideo(JsonElement json)
        {
            Url = json.GetPropertyOrNull("url")?.GetString();
            Width = json.GetPropertyOrNull("width")?.GetInt32();
            Height = json.GetPropertyOrNull("height")?.GetInt32();
        }

        public override string ToString()
        {
            return Url ?? base.ToString();
        }
    }
}
