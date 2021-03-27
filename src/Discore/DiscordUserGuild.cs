namespace Discore
{
    /// <summary>
    /// A brief version of a guild object.
    /// </summary>
    public sealed class DiscordUserGuild : DiscordIdEntity
    {
        /// <summary>
        /// Gets the name of this guild.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the icon of this guild or null if the guild has no icon set.
        /// </summary>
        public DiscordCdnUrl Icon { get; }
        /// <summary>
        /// Gets whether the user is the owner of this guild.
        /// </summary>
        public bool IsOwner { get; }
        /// <summary>
        /// Gets the user's enabled/disabled permissions.
        /// </summary>
        public DiscordPermission Permissions { get; }

        internal DiscordUserGuild(DiscordApiData data)
            : base(data)
        {
            Name = data.GetString("name");
            IsOwner = data.GetBoolean("owner").Value;

            string iconHash = data.GetString("icon");
            if (iconHash != null)
                Icon = DiscordCdnUrl.ForGuildIcon(Id, iconHash);

            long permissions = data.GetInt64("permissions").Value;
            Permissions = (DiscordPermission)permissions;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
