namespace Discore.WebSocket
{
    public sealed class DiscordEmbedFooter : DiscordObject
    {
        /// <summary>
        /// Gets the footer text.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Gets the url of the footer icon (only http(s)).
        /// </summary>
        public string IconUrl { get; private set; }

        /// <summary>
        /// Gets a proxied url of the footer icon.
        /// </summary>
        public string ProxyIconUrl { get; private set; }

        internal DiscordEmbedFooter() { }

        internal override void Update(DiscordApiData data)
        {
            Text = data.GetString("text") ?? Text;
            IconUrl = data.GetString("icon_url") ?? IconUrl;
            ProxyIconUrl = data.GetString("proxy_icon_url") ?? ProxyIconUrl;
        }
    }
}
