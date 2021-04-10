using System;

#nullable enable

namespace Discore
{
    public class DiscordImageData
    {
        /// <summary>
        /// Gets a <see cref="DiscordImageData"/> instance representing a cleared image. 
        /// This can be used to remove avatars from guilds, users, etc.
        /// </summary>
        public static readonly DiscordImageData None = new DiscordImageData();

        /// <summary>
        /// Gets the image data as a base64 encoded string.
        /// </summary>
        public string? Base64Data => base64Data;
        /// <summary>
        /// Gets the media type of the image data (e.g. image/jpeg).
        /// </summary>
        public string? MediaType => mediaType;

        readonly string? base64Data;
        readonly string? mediaType;

        private DiscordImageData()
        {
            base64Data = null;
            mediaType = null;
        }

        /// <summary>
        /// Creates image data from the base64 encoded string of an image.
        /// </summary>
        /// <param name="mediaType">A supported media type (e.g. image/jpeg, image/png, or image/gif).</param>
        public DiscordImageData(string base64Data, string mediaType)
        {
            this.base64Data = base64Data;
            this.mediaType = mediaType;
        }

        /// <summary>
        /// Creates image data from raw image data.
        /// </summary>
        /// <param name="mediaType">A supported media type (e.g. image/jpeg, image/png, or image/gif).</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="imageData"/> is null.</exception>
        public DiscordImageData(byte[] imageData, string mediaType)
        {
            base64Data = Convert.ToBase64String(imageData);
            this.mediaType = mediaType;
        }

        /// <summary>
        /// Creates image data from raw image data.
        /// </summary>
        /// <param name="mediaType">A supported media type (e.g. image/jpeg, image/png, or image/gif).</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="imageData"/> is null.</exception>
        public DiscordImageData(ArraySegment<byte> imageData, string mediaType)
        {
            if (imageData == null) throw new ArgumentNullException(nameof(imageData));

            base64Data = Convert.ToBase64String(imageData.Array, imageData.Offset, imageData.Count);
            this.mediaType = mediaType;
        }

        /// <summary>
        /// Converts the image data to the following format:
        /// <para>data:MEDIA_TYPE;base64,BASE64_IMAGE_DATA</para>
        /// </summary>
        /// <returns>A data URI or null if this is <see cref="None"/>.</returns>
        public string? ToDataUriScheme()
        {
            return string.IsNullOrWhiteSpace(base64Data) ? null : $"data:{mediaType};base64,{base64Data}";
        }

        /// <exception cref="ArgumentException">Thrown if the given data URI is not a valid format.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static DiscordImageData FromDataUriScheme(string dataUri)
        {
            if (dataUri == null)
                throw new ArgumentNullException(nameof(dataUri));

            int colonIndex = dataUri.IndexOf(':');
            if (colonIndex < 0)
                throw new ArgumentException("String is not a valid data URI.");

            int commaIndex = dataUri.IndexOf(',');
            if (commaIndex < 0)
                throw new ArgumentException("String is not a valid data URI.");

            string mediaTypeList = dataUri.Substring(colonIndex, commaIndex - colonIndex);
            int base64Index = mediaTypeList.IndexOf(";base64");
            if (base64Index < 0)
                throw new ArgumentException("Data URI must contain the base64 extension (i.e. \";base64\" before the data).");

            string mediaType = mediaTypeList.Substring(0, base64Index);
            string base64Data = dataUri.Substring(colonIndex + 1);

            return new DiscordImageData(base64Data, mediaType);
        }
    }
}

#nullable restore
