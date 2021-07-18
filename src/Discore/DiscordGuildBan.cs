using System.Text.Json;

namespace Discore
{
    public class DiscordGuildBan
    {
        /// <summary>
        /// Gets the reason for the ban or null if there was no reason.
        /// </summary>
        public string? Reason { get; }

        /// <summary>
        /// Gets the user that was banned.
        /// </summary>
        public DiscordUser User { get; }

        internal DiscordGuildBan(JsonElement json)
        {
            Reason = json.GetPropertyOrNull("reason")?.GetString();
            User = new DiscordUser(json.GetProperty("user"), isWebhookUser: false);
        }
    }
}
