using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

#nullable enable

namespace Discore.Http
{
    public class EmbedOptions
    {
        public class EmbedFooter
        {
            /// <summary>
            /// Gets or sets the text content of the footer.
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Gets or sets the URL of the icon to display in the footer (or null to omit).
            /// <para>To use attachments uploaded alongside the embed, use the format: attachment://FILENAME_WITH_EXT</para>
            /// </summary>
            public string? IconUrl { get; set; }

            public EmbedFooter(string text, string? iconUrl = null)
            {
                Text = text;
                IconUrl = iconUrl;
            }

            internal void Build(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();

                if (Text != null)
                    writer.WriteString("text", Text);
                if (IconUrl != null)
                    writer.WriteString("icon_url", IconUrl);

                writer.WriteEndObject();
            }
        }

        public class EmbedAuthor
        {
            /// <summary>
            /// Gets or sets the author's name.
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// Gets or sets the URL to the author (or null to omit).
            /// </summary>
            public string? Url { get; set; }

            /// <summary>
            /// Gets or sets the URL to the icon of the author (or null to omit).
            /// <para>To use attachments uploaded alongside the embed, use the format: attachment://FILENAME_WITH_EXT</para>
            /// </summary>
            public string? IconUrl { get; set; }

            public EmbedAuthor(string? name, string? url = null, string? iconUrl = null)
            {
                Name = name;
                Url = url;
                IconUrl = iconUrl;
            }

            internal void Build(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();

                if (Name != null)
                    writer.WriteString("name", Name);
                if (Url != null)
                    writer.WriteString("url", Url);
                if (IconUrl != null)
                    writer.WriteString("icon_url", IconUrl);

                writer.WriteEndObject();
            }
        }

        public class EmbedField
        {
            /// <summary>
            /// Gets or sets the name of the field.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the value of the field.
            /// </summary>
            public string Value { get; set; }
            
            /// <summary>
            /// Gets or sets whether the field should display inline with other inline fields.
            /// </summary>
            public bool IsInline { get; set; }

            public EmbedField(string name, string value, bool isInline = false)
            {
                Name = name;
                Value = value;
                IsInline = isInline;
            }

            internal void Build(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();

                if (Name != null)
                    writer.WriteString("name", Name);
                if (Value != null)
                    writer.WriteString("value", Value);
                
                writer.WriteBoolean("inline", IsInline);

                writer.WriteEndObject();
            }
        }

        /// <summary>
        /// Gets or sets the title of the embed (or null to omit).
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the embed (or null to omit).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the URL that the embed links to (or null to omit).
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or sets the timestamp on the embed (or null to omit).
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the color of the embed (or null to use default).
        /// </summary>
        public DiscordColor? Color { get; set; }

        /// <summary>
        /// Gets or sets the footer of the embed (or null to omit).
        /// </summary>
        public EmbedFooter? Footer { get; set; }

        /// <summary>
        /// Gets or sets the URL of the image to include in the embed (or null to omit).
        /// <para>To use attachments uploaded alongside the embed, use the format: attachment://FILENAME_WITH_EXT</para>
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the author of the embed (or null to omit).
        /// </summary>
        public EmbedAuthor? Author { get; set; }

        /// <summary>
        /// Gets or sets the URL of the thumbnail for the embed (or null to omit).
        /// </summary>
        public string? ThumbnailUrl { get; set; }

        /// <summary>
        /// Gets or sets the fields to include in the embed (or null to omit).
        /// </summary>
        public IList<EmbedField>? Fields { get; set; }

        /// <summary>
        /// Sets the title of the embed.
        /// </summary>
        public EmbedOptions SetTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>
        /// Sets the description of the embed.
        /// </summary>
        public EmbedOptions SetDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        /// Sets the URL that the embed links to.
        /// </summary>
        public EmbedOptions SetUrl(string url)
        {
            Url = url;
            return this;
        }

        /// <summary>
        /// Sets the timestamp on the embed.
        /// </summary>
        public EmbedOptions SetTimestamp(DateTime timestamp)
        {
            Timestamp = timestamp;
            return this;
        }

        /// <summary>
        /// Sets the color of the embed.
        /// </summary>
        public EmbedOptions SetColor(DiscordColor color)
        {
            Color = color;
            return this;
        }

        /// <summary>
        /// Sets the footer of the embed.
        /// </summary>
        /// <param name="iconUrl">
        /// The URL of the icon to display in the footer.
        /// <para>To use attachments uploaded alongside the embed, use the format: attachment://FILENAME_WITH_EXT</para>
        /// </param>
        public EmbedOptions SetFooter(string text, string? iconUrl = null)
        {
            Footer = new EmbedFooter(text, iconUrl);
            return this;
        }

        /// <summary>
        /// Sets the URL of the image to include in the embed.
        /// <para>To use attachments uploaded alongside the embed, use the format: attachment://FILENAME_WITH_EXT</para>
        /// </summary>
        public EmbedOptions SetImage(string imageUrl)
        {
            ImageUrl = imageUrl;
            return this;
        }

        /// <summary>
        /// Sets the author of the embed.
        /// </summary>
        /// <param name="iconUrl">
        /// The URL of the author's icon.
        /// <para>To use attachments uploaded alongside the embed, use the format: attachment://FILENAME_WITH_EXT</para>
        /// </param>
        public EmbedOptions SetAuthor(string? name, string? url = null, string? iconUrl = null)
        {
            Author = new EmbedAuthor(name, url, iconUrl);
            return this;
        }

        /// <summary>
        /// Sets the URL of the thumbnail for the embed.
        /// <para>To use attachments uploaded alongside the embed, use the format: attachment://FILENAME_WITH_EXT</para>
        /// </summary>
        public EmbedOptions SetThumbnail(string thumbnailUrl)
        {
            ThumbnailUrl = thumbnailUrl;
            return this;
        }

        /// <summary>
        /// Adds a field to the embed.
        /// </summary>
        /// <param name="inline">Whether the field should display inline with other inline fields.</param>
        public EmbedOptions AddField(string name, string value, bool inline = false)
        {
            if (Fields == null)
                Fields = new List<EmbedField>();

            Fields.Add(new EmbedField(name, value, inline));
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            if (Title != null)
                writer.WriteString("title", Title);
            if (Description != null)
                writer.WriteString("description", Description);
            if (Url != null)
                writer.WriteString("url", Url);
            if (Timestamp.HasValue)
                writer.WriteString("timestamp", Timestamp.Value.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture));
            if (Color.HasValue)
                writer.WriteNumber("color", Color.Value.ToHexadecimal());

            if (Footer != null)
            {
                writer.WritePropertyName("footer");
                Footer.Build(writer);
            }

            if (ImageUrl != null)
            {
                writer.WriteStartObject("image");
                writer.WriteString("url", ImageUrl);
                writer.WriteEndObject();
            }

            if (Author != null)
            {
                writer.WritePropertyName("author");
                Author.Build(writer);
            }

            if (ThumbnailUrl != null)
            {
                writer.WriteStartObject("thumbnail");
                writer.WriteString("url", ThumbnailUrl);
                writer.WriteEndObject();
            }

            if (Fields != null)
            {
                writer.WriteStartArray("fields");

                foreach (EmbedField field in Fields)
                    field.Build(writer);

                writer.WriteEndArray();
            }

            writer.WriteEndObject();
        }
    }
}

#nullable restore
