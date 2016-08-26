namespace Discore
{
    /// <summary>
    /// A provider of a <see cref="DiscordEmbed"/>.
    /// </summary>
    public class DiscordEmbedProvider : IDiscordObject
    {
        /// <summary>
        /// Gets the name of this provider.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the url of this provider.
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// Updates this embed provider with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this embed provider with.</param>
        public void Update(DiscordApiData data)
        {
            Name = data.GetString("name") ?? Name;
            Url = data.GetString("url") ?? Url;
        }
    }
}
