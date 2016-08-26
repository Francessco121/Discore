namespace Discore
{
    public class DiscordEmbedThumbnail : IDiscordObject
    {
        public string Url { get; private set; }
        public string ProxyUrl { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public void Update(DiscordApiData data)
        {
            Url = data.GetString("url") ?? Url;
            ProxyUrl = data.GetString("proxy_url") ?? ProxyUrl;
            Width = data.GetInteger("width") ?? Width;
            Height = data.GetInteger("height") ?? Height;
        }
    }
}
