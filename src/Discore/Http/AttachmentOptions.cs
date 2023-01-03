using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Discore.Http
{
    public class AttachmentOptions
    {
        /// <summary>
        /// Gets or sets the ID of this attachment.
        /// </summary>
        public Snowflake Id { get; set; }
        /// <summary>
        /// Gets or sets the file name of this attachment.
        /// </summary>
        public string? FileName { get; set; }
        /// <summary>
        /// Gets or sets the description of this attachment.
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Gets or sets the actual attachment content to upload.
        /// Does not need to be set when keeping an existing attachment or only modifying
        /// other properties like the description.
        /// </summary>
        public HttpContent? Content { get; set; }

        /// <summary>
        /// Creates attachment options for a new or existing attachment.
        /// </summary>
        /// <param name="id">
        /// When uploading a new attachment, should be a temporary unique ID such as 0, 1, 2, etc.
        /// When modifying or keeping an existing attachment (for message edits), should be its full generated snowflake ID.
        /// </param>
        public AttachmentOptions(Snowflake id)
        {
            Id = id;
        }

        /// <summary>
        /// Sets the file name of this attachment.
        /// </summary>
        public AttachmentOptions SetFileName(string? fileName)
        {
            FileName = fileName;
            return this;
        }

        /// <summary>
        /// Sets the description of this attachment.
        /// </summary>
        public AttachmentOptions SetDescription(string? description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        /// Sets the actual attachment content to upload.
        /// </summary>
        public AttachmentOptions SetContent(HttpContent? content)
        {
            Content = content;
            return this;
        }

        /// <summary>
        /// Sets the actual attachment content to upload.
        /// </summary>
        public AttachmentOptions SetContent(Stream content, string? mediaType = null)
        {
            var streamContent = new StreamContent(content);

            if (mediaType != null)
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            Content = streamContent;

            return this;
        }

        /// <summary>
        /// Sets the actual attachment content to upload.
        /// </summary>
        public AttachmentOptions SetContent(ArraySegment<byte> content, string? mediaType = null)
        {
            var byteContent = new ByteArrayContent(content.Array, content.Offset, content.Count);

            if (mediaType != null)
                byteContent.Headers.ContentType = new MediaTypeHeaderValue(mediaType);

            Content = byteContent;

            return this;
        }

        /// <summary>
        /// Sets the actual attachment content to upload.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="mediaType">Defaults to text/plain.</param>
        /// <param name="encoding">Defaults to UTF8.</param>
        public AttachmentOptions SetContent(string content, string? mediaType = null, Encoding? encoding = null)
        {
            Content = new StringContent(content, encoding ?? Encoding.UTF8, mediaType ?? "text/plain");
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteSnowflake("id", Id);

            if (FileName != null)
                writer.WriteString("filename", FileName);
            if (Description != null)
                writer.WriteString("description", Description);

            writer.WriteEndObject();
        }
    }
}
