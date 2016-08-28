namespace Discore
{
    /// <summary>
    /// An attachment in a <see cref="DiscordMessage"/>.
    /// </summary>
    public class DiscordAttachment : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of the attachment.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the file name of the attachment.
        /// </summary>
        public string FileName { get; private set; }
        /// <summary>
        /// Gets the byte file-size of the attachment.
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// Gets the url of this attachment.
        /// </summary>
        public string Url { get; private set; }
        /// <summary>
        /// Gets the proxy url of this attachment.
        /// </summary>
        public string ProxyUrl { get; private set; }
        /// <summary>
        /// Gets the pixel-width of this attachment.
        /// </summary>
        public int? Width { get; private set; }
        /// <summary>
        /// Gets the pixel-height of this attachment.
        /// </summary>
        public int? Height { get; private set; }

        /// <summary>
        /// Updates this attachment with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this attachment with.</param>
        public void Update(DiscordApiData data)
        {
            Id       = data.GetString("id") ?? Id;
            FileName = data.GetString("filename") ?? FileName;
            Size     = data.GetInteger("size") ?? Size;
            Url      = data.GetString("url") ?? Url;
            ProxyUrl = data.GetString("proxy_url") ?? ProxyUrl;
            Width    = data.GetInteger("width") ?? Width;
            Height   = data.GetInteger("height") ?? Height;
        }

        #region Object Overrides
        /// <summary>
        /// Determines whether the specified <see cref="DiscordAttachment"/> is equal 
        /// to the current attachment.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordAttachment"/> to check.</param>
        public bool Equals(DiscordAttachment other)
        {
            return Id == other?.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current attachment.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordAttachment other = obj as DiscordAttachment;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this attachment.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns the file name of this attachment.
        /// </summary>
        public override string ToString()
        {
            return FileName;
        }

#pragma warning disable 1591
        public static bool operator ==(DiscordAttachment a, DiscordAttachment b)
        {
            return a?.Id == b?.Id;
        }

        public static bool operator !=(DiscordAttachment a, DiscordAttachment b)
        {
            return a?.Id != b?.Id;
        }
#pragma warning restore 1591
        #endregion
    }
}
