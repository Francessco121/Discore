#nullable enable

using System;
using System.Text.Json;

namespace Discore
{
    public sealed class DiscordInviteGuild
    {
        // TODO: Rename to Id
        /// <summary>
        /// Gets the ID of the guild this invite is for.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// Gets the name of the guild.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the hash of the guild splash (or null if none exists).
        /// </summary>
        public string? SplashHash { get; }

        // TODO: add splash, banner, description, icon, features, verification_level, vanity_url_code

        public DiscordInviteGuild(Snowflake guildId, string name, string? splashHash)
        {
            GuildId = guildId;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            SplashHash = splashHash;
        }

        internal DiscordInviteGuild(JsonElement json)
        {
            GuildId = json.GetProperty("id").GetSnowflake();
            Name = json.GetProperty("name").GetString()!;
            SplashHash = json.GetPropertyOrNull("splash")?.GetString();
        }
    }
}

#nullable restore
