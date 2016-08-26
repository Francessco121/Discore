namespace Discore
{
    /// <summary>
    /// Represents an Offline Guild, or a Guild whose information has not been provided 
    /// through Guild Create events during the Gateway connect.
    /// </summary>
    public class DiscordUnavailableGuild : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of the guild.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets whether or not this guild is unavailable.
        /// </summary>
        public bool Unavailable { get; private set; }

        /// <summary>
        /// Updates this unavailable guild with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this unavailable guild with.</param>
        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Unavailable = data.GetBoolean("unavailable") ?? Unavailable;
        }
    }
}
