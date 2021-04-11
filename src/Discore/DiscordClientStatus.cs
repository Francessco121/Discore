using System.Text.Json;

namespace Discore
{
    public sealed class DiscordClientStatus
    {
        /// <summary>
        /// The user's status set for an active desktop (Windows, Linux, Mac) application session.
        /// </summary>
        public string? Desktop { get; }

        /// <summary>
        /// The user's status set for an active mobile (iOS, Android) application session.
        /// </summary>
        public string? Mobile { get; }

        /// <summary>
        /// The user's status set for an active web (browser, bot account) application session.
        /// </summary>
        public string? Web { get; }

        public DiscordClientStatus(string? desktop, string? mobile, string? web)
        {
            Desktop = desktop;
            Mobile = mobile;
            Web = web;
        }

        internal DiscordClientStatus(JsonElement json)
        {
            Desktop = json.GetPropertyOrNull("desktop")?.GetString();
            Mobile = json.GetPropertyOrNull("mobile")?.GetString();
            Web = json.GetPropertyOrNull("web")?.GetString();
        }
    }
}
