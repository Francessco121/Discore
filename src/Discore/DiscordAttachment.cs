namespace Discore
{
    public class DiscordAttachment : IDiscordObject, ICacheable
    {
        public string Id { get; private set; }
        public string FileName { get; private set; }
        public int Size { get; private set; }
        public string Url { get; private set; }
        public string ProxyUrl { get; private set; }
        public int? Width { get; private set; }
        public int? Height { get; private set; }

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
    }
}
