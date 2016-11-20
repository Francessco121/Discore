using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Discore.WebSocket
{
    /// <summary>
    /// Embedded content in a message.
    /// </summary>
    public sealed class DiscordEmbed : DiscordObject
    {
        /// <summary>
        /// Gets the title of this embed.
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// Gets the type of this embed.
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// Gets the description of this embed.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Gets the url of this embed.
        /// </summary>
        public string Url { get; private set; }
        /// <summary>
        /// Gets the timestamp of this embed.
        /// </summary>
        public DateTime Timestamp { get; private set; }
        /// <summary>
        /// Gets the color code of this embed.
        /// </summary>
        public int Color { get; private set; }
        /// <summary>
        /// Gets the footer information.
        /// </summary>
        public DiscordEmbedFooter Footer { get; private set; }
        /// <summary>
        /// Gets the image information.
        /// </summary>
        public DiscordEmbedImage Image { get; private set; }
        /// <summary>
        /// Gets the thumbnail of this embed.
        /// </summary>
        public DiscordEmbedThumbnail Thumbnail { get; private set; }
        /// <summary>
        /// Gets the video information.
        /// </summary>
        public DiscordEmbedVideo Video { get; private set; }
        /// <summary>
        /// Gets the provider of this embed.
        /// </summary>
        public DiscordEmbedProvider Provider { get; private set; }
        /// <summary>
        /// Gets the author information.
        /// </summary>
        public DiscordEmbedAuthor Author { get; private set; }
        /// <summary>
        /// Gets a list of all fields in this embed.
        /// </summary>
        public IReadOnlyList<DiscordEmbedField> Fields { get; private set; }

        internal DiscordEmbed() { }

        internal override void Update(DiscordApiData data)
        {
            Title = data.GetString("title") ?? Title;
            Type = data.GetString("type") ?? Type;
            Description = data.GetString("description") ?? Description;
            Url = data.GetString("url") ?? Url;
            Timestamp = data.GetDateTime("timestamp") ?? Timestamp;
            Color = data.GetInteger("color") ?? Color;

            DiscordApiData footerData = data.Get("footer");
            if (footerData != null)
            {
                if (Footer == null)
                    Footer = new DiscordEmbedFooter();

                Footer.Update(footerData);
            }

            DiscordApiData imageData = data.Get("image");
            if (imageData != null)
            {
                if (Image == null)
                    Image = new DiscordEmbedImage();

                Image.Update(imageData);
            }

            DiscordApiData thumbnailData = data.Get("thumbnail");
            if (thumbnailData != null)
            {
                if (Thumbnail == null)
                    Thumbnail = new DiscordEmbedThumbnail();

                Thumbnail.Update(thumbnailData);
            }

            DiscordApiData videoData = data.Get("video");
            if (videoData != null)
            {
                if (Video == null)
                    Video = new DiscordEmbedVideo();

                Video.Update(videoData);
            }

            DiscordApiData providerData = data.Get("provider");
            if (providerData != null)
            {
                if (Provider == null)
                    Provider = new DiscordEmbedProvider();

                Provider.Update(providerData);
            }

            DiscordApiData authorData = data.Get("author");
            if (authorData != null)
            {
                if (Author == null)
                    Author = new DiscordEmbedAuthor();

                Author.Update(authorData);
            }

            IList<DiscordApiData> fieldArray = data.GetArray("fields");
            if (fieldArray != null)
            {
                DiscordEmbedField[] fields = new DiscordEmbedField[fieldArray.Count];
                for (int i = 0; i < fields.Length; i++)
                {
                    DiscordEmbedField field = new DiscordEmbedField();
                    field.Update(fieldArray[i]);

                    fields[i] = field;
                }

                Fields = new ReadOnlyCollection<DiscordEmbedField>(fields);
            }
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
