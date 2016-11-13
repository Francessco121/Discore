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
        public string Name { get; private set; }
        /// <summary>
        /// Gets the icon hash of this user guild.
        /// </summary>
        public string Icon { get; private set; }
        /// <summary>
        /// Gets whether the user is the owner of this user guild.
        /// </summary>
        public bool IsOwner { get; private set; }
        /// <summary>
        /// Gets the user's enabled/disabled permissions.
        /// </summary>
        public DiscordPermission Permissions { get; private set; }

        internal DiscordUserGuild() { }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            Name = data.GetString("name") ?? Name;
            Icon = data.GetString("icon") ?? Icon;
            IsOwner = data.GetBoolean("owner") ?? IsOwner;

            long? permissions = data.GetInt64("permissions");
            if (permissions.HasValue)
                Permissions = (DiscordPermission)permissions.Value;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
