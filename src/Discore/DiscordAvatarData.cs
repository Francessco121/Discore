using System;

namespace Discore
{
    public class DiscordAvatarData
    {
        /// <summary>
        /// Gets the avatar data as a base64 encoded string.
        /// </summary>
        public string Base64AvatarData => base64Data;
        /// <summary>
        /// Gets the media type of the avatar data (e.g. image/jpeg).
        /// </summary>
        public string MediaType => mediaType;

        string base64Data;
        string mediaType;

        /// <summary>
        /// Creates avatar data from the base64 encoded string of an image.
        /// </summary>
        /// <param name="mediaType">A supported media type (e.g. image/jpeg, image/png, or image/gif).</param>
        public DiscordAvatarData(string base64Data, string mediaType)
        {
            this.base64Data = base64Data;
            this.mediaType = mediaType;
        }

        /// <summary>
        /// Creates avatar data from raw image data.
        /// </summary>
        /// <param name="mediaType">A supported media type (e.g. image/jpeg, image/png, or image/gif).</param>
        /// <exception cref="ArgumentNullException">Thrown if the array is null.</exception>
        public DiscordAvatarData(byte[] imageData, string mediaType)
        {
            base64Data = Convert.ToBase64String(imageData);
            this.mediaType = mediaType;
        }

        /// <summary>
        /// Creates avatar data from raw image data.
        /// </summary>
        /// <param name="mediaType">A supported media type (e.g. image/jpeg, image/png, or image/gif).</param>
        public DiscordAvatarData(ArraySegment<byte> imageData, string mediaType)
        {
            base64Data = Convert.ToBase64String(imageData.Array, imageData.Offset, imageData.Count);
            this.mediaType = mediaType;
        }

        /// <summary>
        /// Converts avatar data to the following format:
        /// <para>data:MEDIA_TYPE;base64,BASE64_IMAGE_DATA</para>
        /// </summary>
        [Obsolete("Use ToDataUriScheme() instead.")]
        public string ToFormattedString()
        {
            return ToDataUriScheme();
        }

        /// <summary>
        /// Converts avatar data to the following format:
        /// <para>data:MEDIA_TYPE;base64,BASE64_IMAGE_DATA</para>
        /// </summary>
        public string ToDataUriScheme()
        {
            return $"data:{mediaType};base64,{base64Data}";
        }

        /// <exception cref="ArgumentException">Thrown if the given data URI is not a valid format.</exception>
        public static DiscordAvatarData FromDataUriScheme(string dataUri)
        {
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

            return new DiscordAvatarData(base64Data, mediaType);
        }
    }
}
