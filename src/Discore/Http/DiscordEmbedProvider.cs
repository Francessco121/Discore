namespace Discore.Http
{
    /// <summary>
    /// The web provider of a <see cref="DiscordEmbed"/>.
    /// </summary>
    public class DiscordEmbedProvider
    {
        /// <summary>
        /// Gets the name of this provider.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the url of this provider.
        /// </summary>
        public string Url { get; }

        public DiscordEmbedProvider(DiscordApiData data)
        {
            Name = data.GetString("name");
            Url = data.GetString("url");
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
