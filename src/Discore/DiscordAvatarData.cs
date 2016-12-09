using System;

namespace Discore
{
    public class DiscordAvatarData
    {
        /// <summary>
        /// Gets the avatar data as a base64 encoded string.
        /// </summary>
        public string Base64AvatarData { get { return base64Jpeg; } }

        string base64Jpeg;

        public DiscordAvatarData(string base64Jpeg)
        {
            this.base64Jpeg = base64Jpeg;
        }

        public DiscordAvatarData(byte[] jpegData)
        {
            base64Jpeg = Convert.ToBase64String(jpegData);
        }

        public DiscordAvatarData(ArraySegment<byte> jpegData)
        {
            base64Jpeg = Convert.ToBase64String(jpegData.Array, jpegData.Offset, jpegData.Count);
        }

        /// <summary>
        /// Converts avatar data to the following format:
        /// <para>data:image/jpeg;base64,BASE64_IMAGE_DATA</para>
        /// </summary>
        public string ToFormattedString()
        {
            return $"data:image/jpeg;base64,{base64Jpeg}";
        }
    }
}
