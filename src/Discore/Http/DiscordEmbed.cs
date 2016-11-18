namespace Discore.Http
{
    /// <summary>
    /// Embedded content in a message.
    /// </summary>
    public class DiscordEmbed
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
        /// Gets the thumbnail of this embed.
        /// </summary>
        public DiscordEmbedThumbnail Thumbnail { get; }
        /// <summary>
        /// Gets the provider of this embed.
        /// </summary>
        public DiscordEmbedProvider Provider { get; }

        // TODO: add rest of embed structure

        public DiscordEmbed(DiscordApiData data)
        {
            Title = data.GetString("title");
            Type = data.GetString("type");
            Description = data.GetString("description");
            Url = data.GetString("url");

            DiscordApiData thumbnailData = data.Get("thumbnail");
            if (thumbnailData != null)
                Thumbnail = new DiscordEmbedThumbnail(thumbnailData);

            DiscordApiData providerData = data.Get("provider");
            if (providerData != null)
                Provider = new DiscordEmbedProvider(providerData);
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
