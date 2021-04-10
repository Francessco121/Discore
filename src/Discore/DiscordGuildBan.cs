using System;
using System.Text.Json;

#nullable enable

namespace Discore
{
    public sealed class DiscordGuildBan
    {
        /// <summary>
        /// Gets the reason for the ban or null if there was no reason.
        /// </summary>
        public string? Reason { get; }

        /// <summary>
        /// Gets the user that was banned.
        /// </summary>
        public DiscordUser User { get; }

        /// <exception cref="ArgumentNullException">Thrown if <paramref name="user"/> is null.</exception>
        public DiscordGuildBan(string? reason, DiscordUser user)
        {
            Reason = reason;
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        internal DiscordGuildBan(JsonElement json)
        {
            Reason = json.GetPropertyOrNull("reason")?.GetString();
            User = new DiscordUser(json.GetProperty("user"), isWebhookUser: false);
        }
    }
}

#nullable restore
