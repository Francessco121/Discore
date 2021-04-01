using System;
using System.Collections.Generic;
using System.Text.Json;

#nullable enable

namespace Discore
{
    /// <summary>
    /// Embedded content in a message.
    /// </summary>
    public sealed class DiscordEmbed
    {
        /// <summary>
        /// Gets the title of this embed.
        /// </summary>
        public string? Title { get; }
        /// <summary>
        /// Gets the type of this embed.
        /// </summary>
        public string? Type { get; }
        /// <summary>
        /// Gets the description of this embed.
        /// </summary>
        public string? Description { get; }
        /// <summary>
        /// Gets the url of this embed.
        /// </summary>
        public string? Url { get; }
        /// <summary>
        /// Gets the timestamp of this embed.
        /// </summary>
        public DateTime? Timestamp { get; }
        // TODO: Should this be a DiscordColor?
        /// <summary>
        /// Gets the color code of this embed.
        /// </summary>
        public int? Color { get; }
        /// <summary>
        /// Gets the footer information.
        /// </summary>
        public DiscordEmbedFooter? Footer { get; }
        /// <summary>
        /// Gets the image information.
        /// </summary>
        public DiscordEmbedImage? Image { get; }
        /// <summary>
        /// Gets the thumbnail of this embed.
        /// </summary>
        public DiscordEmbedThumbnail? Thumbnail { get; }
        /// <summary>
        /// Gets the video information.
        /// </summary>
        public DiscordEmbedVideo? Video { get; }
        /// <summary>
        /// Gets the provider of this embed.
        /// </summary>
        public DiscordEmbedProvider? Provider { get; }
        /// <summary>
        /// Gets the author information.
        /// </summary>
        public DiscordEmbedAuthor? Author { get; }
        /// <summary>
        /// Gets a list of all fields in this embed.
        /// </summary>
        public IReadOnlyList<DiscordEmbedField>? Fields { get; }

        public DiscordEmbed(
            string? title, 
            string? type, 
            string? description, 
            string? url, 
            DateTime? timestamp, 
            int? color, 
            DiscordEmbedFooter? footer, 
            DiscordEmbedImage? image, 
            DiscordEmbedThumbnail? thumbnail, 
            DiscordEmbedVideo? video, 
            DiscordEmbedProvider? provider, 
            DiscordEmbedAuthor? author, 
            IReadOnlyList<DiscordEmbedField>? fields)
        {
            Title = title;
            Type = type;
            Description = description;
            Url = url;
            Timestamp = timestamp;
            Color = color;
            Footer = footer;
            Image = image;
            Thumbnail = thumbnail;
            Video = video;
            Provider = provider;
            Author = author;
            Fields = fields;
        }

        internal DiscordEmbed(JsonElement json)
        {
            Title = json.GetPropertyOrNull("title")?.GetString();
            Type = json.GetPropertyOrNull("type")?.GetString();
            Description = json.GetPropertyOrNull("description")?.GetString();
            Url = json.GetPropertyOrNull("url")?.GetString();
            Timestamp = json.GetPropertyOrNull("timestamp")?.GetDateTime();
            Color = json.GetPropertyOrNull("color")?.GetInt32();

            JsonElement? footerJson = json.GetPropertyOrNull("footer");
            if (footerJson != null)
                Footer = new DiscordEmbedFooter(footerJson.Value);

            JsonElement? imageJson = json.GetPropertyOrNull("image");
            if (imageJson != null)
                Image = new DiscordEmbedImage(imageJson.Value);

            JsonElement? thumbnailJson = json.GetPropertyOrNull("thumbnail");
            if (thumbnailJson != null)
                Thumbnail = new DiscordEmbedThumbnail(thumbnailJson.Value);

            JsonElement? videoJson = json.GetPropertyOrNull("video");
            if (videoJson != null)
                Video = new DiscordEmbedVideo(videoJson.Value);

            JsonElement? providerJson = json.GetPropertyOrNull("provider");
            if (providerJson != null)
                Provider = new DiscordEmbedProvider(providerJson.Value);

            JsonElement? authorJson = json.GetPropertyOrNull("author");
            if (authorJson != null)
                Author = new DiscordEmbedAuthor(authorJson.Value);

            JsonElement? fieldsJson = json.GetPropertyOrNull("fields");
            if (fieldsJson != null)
            {
                JsonElement _fieldsJson = fieldsJson.Value;
                var fields = new DiscordEmbedField[_fieldsJson.GetArrayLength()];

                for (int i = 0; i < fields.Length; i++)
                    fields[i] = new DiscordEmbedField(_fieldsJson[i]);

                Fields = fields;
            }
        }

        public override string ToString()
        {
            return Title ?? base.ToString();
        }
    }
}

#nullable restore
