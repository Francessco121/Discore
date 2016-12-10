namespace Discore
{
    /// <summary>
    /// The web provider of a <see cref="DiscordEmbed"/>.
    /// </summary>
    public sealed class DiscordEmbedProvider : DiscordSerializable
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

        internal override DiscordApiData Serialize()
        {
            DiscordApiData data = DiscordApiData.CreateContainer();
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
