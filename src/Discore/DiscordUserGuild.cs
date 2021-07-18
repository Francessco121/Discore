using System.Text.Json;

namespace Discore
{
    /// <summary>
    /// A brief version of a guild object.
    /// </summary>
    public class DiscordUserGuild : DiscordIdEntity
    {
        /// <summary>
        /// Gets the name of this guild.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the icon of this guild or null if the guild has no icon set.
        /// </summary>
        public DiscordCdnUrl? Icon { get; }
        /// <summary>
        /// Gets whether the user is the owner of this guild.
        /// </summary>
        public bool IsOwner { get; }
        /// <summary>
        /// Gets the user's enabled/disabled permissions.
        /// </summary>
        public DiscordPermission Permissions { get; }

        // TODO: add: features

        internal DiscordUserGuild(JsonElement json)
            : base(json)
        {
            Name = json.GetProperty("name").GetString()!;
            IsOwner = json.GetProperty("owner").GetBoolean();
            Permissions = (DiscordPermission)json.GetProperty("permissions").GetStringUInt64();

            string? iconStr = json.GetPropertyOrNull("icon")?.GetString();
            Icon = iconStr == null ? null : DiscordCdnUrl.ForGuildIcon(Id, iconStr);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
