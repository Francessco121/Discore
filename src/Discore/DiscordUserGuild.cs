namespace Discore
{
    /// <summary>
    /// A brief version of a guild object.
    /// </summary>
    public class DiscordUserGuild : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of this user guild.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the name of this user guild.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the icon hash of this user guild.
        /// </summary>
        public string Icon { get; private set; }
        /// <summary>
        /// Gets whether or not the user is the owner of this user guild.
        /// </summary>
        public bool Owner { get; private set; }
        /// <summary>
        /// Gets the user's enabled/disabled permissions.
        /// </summary>
        public DiscordPermission Permissions { get; private set; }

        /// <summary>
        /// Updates this user guild with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this user guild with.</param>
        public void Update(DiscordApiData data)
        {
            Id          = data.GetString("id") ?? Id;
            Name        = data.GetString("name") ?? Name;
            Icon        = data.GetString("icon") ?? Icon;
            Owner       = data.GetBoolean("owner") ?? Owner;

            long? permissions = data.GetInt64("permissions");
            if (permissions.HasValue)
                Permissions = (DiscordPermission)permissions.Value;
        }
    }
}
