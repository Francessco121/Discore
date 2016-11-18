namespace Discore.WebSocket
{
    /// <summary>
    /// The web provider of a <see cref="DiscordEmbed"/>.
    /// </summary>
    public sealed class DiscordEmbedProvider : DiscordObject
    {
        /// <summary>
        /// Gets the name of this provider.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the url of this provider.
        /// </summary>
        public string Url { get; private set; }

        internal DiscordEmbedProvider() { }

        internal override void Update(DiscordApiData data)
        {
            Name = data.GetString("name") ?? Name;
            Url = data.GetString("url") ?? Url;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
