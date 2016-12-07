using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Discore.Http
{
    /// <summary>
    /// Embedded content in a message.
    /// </summary>
    public class DiscordEmbed : IDiscordSerializable
    {
        /// <summary>
        /// Gets the title of this embed.
        /// </summary>
        public string Title { get; }
        /// <summary>
        /// Gets the type of this embed.
        /// </summary>
        public string Type { get; }
        /// <summary>
        /// Gets the description of this embed.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Gets the url of this embed.
        /// </summary>
        public string Url { get; }
        /// <summary>
        /// Gets the timestamp of this embed.
        /// </summary>
        public DateTime Timestamp { get; }
        /// <summary>
        /// Gets the color code of this embed.
        /// </summary>
        public DiscordColor Color { get; private set; }
        /// <summary>
        /// Gets the footer information.
        /// </summary>
        public DiscordEmbedFooter Footer { get; }
        /// <summary>
        /// Gets the image information.
        /// </summary>
        public DiscordEmbedImage Image { get; }
        /// <summary>
        /// Gets the thumbnail of this embed.
        /// </summary>
        public DiscordEmbedThumbnail Thumbnail { get; }
        /// <summary>
        /// Gets the video information.
        /// </summary>
        public DiscordEmbedVideo Video { get; }
        /// <summary>
        /// Gets the provider of this embed.
        /// </summary>
        public DiscordEmbedProvider Provider { get; }
        /// <summary>
        /// Gets the author information.
        /// </summary>
        public DiscordEmbedAuthor Author { get; }
        /// <summary>
        /// Gets a list of all fields in this embed.
        /// </summary>
        public IReadOnlyList<DiscordEmbedField> Fields { get; }

        public DiscordEmbed(DiscordApiData data)
        {
            Title = data.GetString("title");
            Type = data.GetString("type");
            Description = data.GetString("description");
            Url = data.GetString("url");
            Timestamp = data.GetDateTime("timestamp") ?? DateTime.UtcNow;
            Color = data.GetColor("color") ?? DiscordColor.DefaultEmbed;

            DiscordApiData footerData = data.Get("footer");
            if (footerData != null)
                Footer = new DiscordEmbedFooter(footerData);

            DiscordApiData imageData = data.Get("image");
            if (imageData != null)
                Image = new DiscordEmbedImage(imageData);

            DiscordApiData thumbnailData = data.Get("thumbnail");
            if (thumbnailData != null)
                Thumbnail = new DiscordEmbedThumbnail(thumbnailData);

            DiscordApiData videoData = data.Get("video");
            if (videoData != null)
                Video = new DiscordEmbedVideo(videoData);

            DiscordApiData providerData = data.Get("provider");
            if (providerData != null)
                Provider = new DiscordEmbedProvider(providerData);

            DiscordApiData authorData = data.Get("author");
            if (authorData != null)
                Author = new DiscordEmbedAuthor(authorData);

            IList<DiscordApiData> fieldArray = data.GetArray("fields");
            if (fieldArray != null)
            {
                DiscordEmbedField[] fields = new DiscordEmbedField[fieldArray.Count];
                for (int i = 0; i < fields.Length; i++)
                    fields[i] = new DiscordEmbedField(fieldArray[i]);

                Fields = new ReadOnlyCollection<DiscordEmbedField>(fields);
            }
        }

        public override string ToString()
        {
            return Title;
        }

        public DiscordApiData Serialize()
        {
            DiscordApiData data = DiscordApiData.CreateContainer();
            data.Set("title", Title);
            data.Set("type", Type);
            data.Set("description", Description);
            data.Set("url", Url);
            data.Set("timestamp", Timestamp);
            data.Set("color", Color);
            data.Set("footer", Footer?.Serialize());
            data.Set("image", Image?.Serialize());
            data.Set("thumbnail", Thumbnail?.Serialize());
            data.Set("video", Video?.Serialize());
            data.Set("provider", Provider?.Serialize());
            data.Set("author", Author?.Serialize());

            if (Fields != null)
            {
                DiscordApiData fields = DiscordApiData.CreateArray();
                foreach (DiscordEmbedField field in Fields)
                    fields.Values.Add(field.Serialize());

                data.Set("fields", fields);
            }

            return data;

        }
    }
}
