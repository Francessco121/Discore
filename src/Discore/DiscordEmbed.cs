namespace Discore
{
    public class DiscordEmbed : IDiscordObject
    {
        public string Title { get; private set; }
        public string Type { get; private set; }
        public string Description { get; private set; }
        public string Url { get; private set; }
        public DiscordEmbedThumbnail Thumbnail { get; private set; }
        public DiscordEmbedProvider Provider { get; private set; }

        public void Update(DiscordApiData data)
        {
            Title = data.GetString("title") ?? Title;
            Type = data.GetString("type") ?? Type;
            Description = data.GetString("description") ?? Description;
            Url = data.GetString("url") ?? Url;

            DiscordApiData thumbnailData = data.Get("thumbnail");
            if (thumbnailData != null)
            {
                if (Thumbnail == null)
                    Thumbnail = new DiscordEmbedThumbnail();

                Thumbnail.Update(thumbnailData);
            }

            DiscordApiData providerData = data.Get("provider");
            if (providerData != null)
            {
                if (Provider == null)
                    Provider = new DiscordEmbedProvider();

                Provider.Update(providerData);
            }
        }
    }
}
