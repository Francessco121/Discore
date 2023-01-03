using System.Text.Json;

namespace Discore
{
    public class DiscordAttachment : DiscordIdEntity
    {
        /// <summary>
        /// Gets the file name of this attachment.
        /// </summary>
        public string FileName { get; }
        /// <summary>
        /// Gets the description of this attachment.
        /// </summary>
        public string? Description { get; }
        /// <summary>
        /// Gets the media type of this attachment.
        /// </summary>
        public string? ContentType { get; }
        /// <summary>
        /// Gets the byte file-size of this attachment.
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
        /// <summary>
        /// Gets whether this attachment is ephemeral.
        /// </summary>
        /// <remarks>
        /// Ephemeral attachments on messages are only guaranteed to be available as long as the message itself exists.
        /// </remarks>
        public bool Ephemeral { get; }

        internal DiscordAttachment(JsonElement json)
            : base(json)
        {
            FileName = json.GetProperty("filename").GetString()!;
            Description = json.GetPropertyOrNull("description")?.GetString()!;
            ContentType = json.GetPropertyOrNull("content_type")?.GetString()!;
            Size = json.GetProperty("size").GetInt32();
            Url = json.GetProperty("url").GetString()!;
            ProxyUrl = json.GetProperty("proxy_url").GetString()!;
            Width = json.GetPropertyOrNull("width")?.GetInt32OrNull();
            Height = json.GetPropertyOrNull("height")?.GetInt32OrNull();
            Ephemeral = json.GetPropertyOrNull("ephemeral")?.GetBoolean() ?? false;
        }

        public override string ToString()
        {
            return FileName;
        }
    }
}
