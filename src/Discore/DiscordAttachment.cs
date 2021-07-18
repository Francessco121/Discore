using System.Text.Json;

namespace Discore
{
    public class DiscordAttachment : DiscordIdEntity
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

        internal DiscordAttachment(JsonElement json)
            : base(json)
        {
            FileName = json.GetProperty("filename").GetString()!;
            Size = json.GetProperty("size").GetInt32();
            Url = json.GetProperty("url").GetString()!;
            ProxyUrl = json.GetProperty("proxy_url").GetString()!;
            Width = json.GetPropertyOrNull("width")?.GetInt32OrNull();
            Height = json.GetPropertyOrNull("height")?.GetInt32OrNull();
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
