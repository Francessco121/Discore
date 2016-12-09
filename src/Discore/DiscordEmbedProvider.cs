namespace Discore
{
    /// <summary>
    /// The web provider of a <see cref="DiscordEmbed"/>.
    /// </summary>
    public sealed class DiscordEmbedProvider : IDiscordSerializable
    {
        /// <summary>
        /// Gets the name of this provider.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the url of this provider.
        /// </summary>
        public string Url { get; }

        internal DiscordEmbedProvider(DiscordApiData data)
        {
            Name = data.GetString("name");
            Url = data.GetString("url");
        }

        public DiscordApiData Serialize()
        {
            DiscordApiData data = DiscordApiData.ContainerType;
            data.Set("name", Name);
            data.Set("url", Url);
            return data;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
