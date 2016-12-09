namespace Discore
{
    public sealed class DiscordEmbedAuthor : IDiscordSerializable
    {
        /// <summary>
        /// Gets the name of the author.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the url to the author.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets the url of an icon of the author (only http(s)).
        /// </summary>
        public string IconUrl { get; }

        /// <summary>
        /// Gets a proxied url to the icon of the author.
        /// </summary>
        public string ProxyIconUrl { get; }

        internal DiscordEmbedAuthor(DiscordApiData data)
        {
            Name         = data.GetString("name");
            Url          = data.GetString("url");
            IconUrl      = data.GetString("icon_url");
            ProxyIconUrl = data.GetString("proxy_icon_url");
        }

        public DiscordApiData Serialize()
        {
            DiscordApiData data = DiscordApiData.ContainerType;
            data.Set("name", Name);
            data.Set("url", Url);
            data.Set("icon_url", IconUrl);
            data.Set("proxy_icon_url", ProxyIconUrl);
            return data;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
