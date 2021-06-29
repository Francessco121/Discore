using System;
using System.Text.Json;

namespace Discore
{
    public class DiscordEmbedFooter
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

        /// <exception cref="ArgumentNullException">Thrown if <paramref name="text"/> is null.</exception>
        public DiscordEmbedFooter(string text, string? iconUrl, string? proxyIconUrl)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
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
