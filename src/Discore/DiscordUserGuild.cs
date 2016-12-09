namespace Discore
{
    /// <summary>
    /// A brief version of a guild object.
    /// </summary>
    public sealed class DiscordUserGuild : DiscordIdObject
    {
        /// <summary>
        /// Gets the name of this user guild.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the icon hash of this user guild.
        /// </summary>
        public string Icon { get; }
        /// <summary>
        /// Gets whether the user is the owner of this user guild.
        /// </summary>
        public bool IsOwner { get; }
        /// <summary>
        /// Gets the user's enabled/disabled permissions.
        /// </summary>
        public DiscordPermission Permissions { get; }

        public DiscordUserGuild(DiscordApiData data)
            : base(data)
        {
            Name = data.GetString("name");
            Icon = data.GetString("icon");
            IsOwner = data.GetBoolean("owner").Value;

            long permissions = data.GetInt64("permissions").Value;
            Permissions = (DiscordPermission)permissions;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
