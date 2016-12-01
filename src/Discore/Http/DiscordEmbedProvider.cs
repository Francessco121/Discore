using System;

namespace Discore.Http
{
    /// <summary>
    /// The web provider of a <see cref="DiscordEmbed"/>.
    /// </summary>
    public class DiscordEmbedProvider : IDiscordSerializable
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

        public DiscordApiData Serialize()
        {
            DiscordApiData data = DiscordApiData.CreateContainer();
            data.Set("name", Name);
            data.Set("url", Url);
            return data;
        }
    }
}
