#nullable enable

using System.Text.Json;

namespace Discore
{
    public sealed class DiscordEmbedFooter
    {
        /// <summary>
        /// Gets the footer text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the url of the footer icon.
        /// </summary>
        public string? IconUrl { get; }

        /// <summary>
        /// Gets a proxied url of the footer icon.
        /// </summary>
        public string? ProxyIconUrl { get; }

        public DiscordEmbedFooter(string text, string? iconUrl, string? proxyIconUrl)
        {
            Text = text;
            IconUrl = iconUrl;
            ProxyIconUrl = proxyIconUrl;
        }

        internal DiscordEmbedFooter(JsonElement json)
        {
            Text = json.GetProperty("text").GetString()!;
            IconUrl = json.GetPropertyOrNull("icon_url")?.GetString();
            ProxyIconUrl = json.GetPropertyOrNull("proxy_icon_url")?.GetString();
        }
    }
}

#nullable restore
