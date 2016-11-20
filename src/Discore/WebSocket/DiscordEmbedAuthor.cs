namespace Discore.WebSocket
{
    public sealed class DiscordEmbedAuthor : DiscordObject
    {
        /// <summary>
        /// Gets the name of the author.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the url to the author.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Gets the url of an icon of the author (only http(s)).
        /// </summary>
        public string IconUrl { get; private set; }

        /// <summary>
        /// Gets a proxied url to the icon of the author.
        /// </summary>
        public string ProxyIconUrl { get; private set; }

        internal DiscordEmbedAuthor() { }

        internal override void Update(DiscordApiData data)
        {
            Name = data.GetString("name") ?? Name;
            Url = data.GetString("url") ?? Url;
            IconUrl = data.GetString("icon_url") ?? IconUrl;
            ProxyIconUrl = data.GetString("proxy_icon_url") ?? ProxyIconUrl;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
