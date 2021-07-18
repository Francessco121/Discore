using System.Text.Json;

namespace Discore
{
    public class DiscordPartialUser : DiscordIdEntity
    {
        /// <summary>
        /// Gets the name of this user.
        /// </summary>
        public string? Username { get; }

        /// <summary>
        /// Gets the user's 4-digit discord-tag.
        /// </summary>
        public string? Discriminator { get; }

        /// <summary>
        /// Gets the user's avatar or null if the user does not have an avatar.
        /// </summary>
        public DiscordCdnUrl? Avatar { get; }

        // TODO: Add system, locale, flags, premium_type, public_flags

        internal DiscordPartialUser(JsonElement json)
            : base(json)
        {
            Username = json.GetPropertyOrNull("username")?.GetString();
            Discriminator = json.GetPropertyOrNull("discriminator")?.GetString();

            string? avatarHash = json.GetPropertyOrNull("avatar")?.GetString();
            Avatar = avatarHash != null ? DiscordCdnUrl.ForUserAvatar(Id, avatarHash) : null;
        }
    }
}
