namespace Discore
{
    /// <summary>
    /// Represents an object in the Discord API.
    /// </summary>
    public interface IDiscordObject
    {
        /// <summary>
        /// Updates this <see cref="IDiscordObject"/> with the given <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The <see cref="DiscordApiData"/> to update this instance with.</param>
        void Update(DiscordApiData data);
    }
}
