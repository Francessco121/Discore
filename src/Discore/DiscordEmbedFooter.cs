namespace Discore
{
    public sealed class DiscordEmbedFooter
    {
        /// <summary>
        /// Gets the footer text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the url of the footer icon (only http(s)).
        /// </summary>
        public string IconUrl { get; }

        /// <summary>
        /// Gets a proxied url of the footer icon.
        /// </summary>
        public string ProxyIconUrl { get; }

        internal DiscordEmbedFooter(DiscordApiData data)
        {
            Text = data.GetString("text");
            IconUrl = data.GetString("icon_url");
            ProxyIconUrl = data.GetString("proxy_icon_url");
        }
    }
}
