using System;
using System.Text.Json;

namespace Discore
{
    /// <summary>
    /// Representation of the game a user is currently playing.
    /// </summary>
    public class DiscordGame
    {
        /// <summary>
        /// Gets the name of the game.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the type of the game.
        /// </summary>
        public DiscordGameType Type { get; }
        /// <summary>
        /// Gets the URL of the stream when the type is set to "Streaming" and the URL is valid.
        /// Otherwise, returns null.
        /// </summary>
        public string? Url { get; }

        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is null.</exception>
        public DiscordGame(
            string name, 
            DiscordGameType type, 
            string? url)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            Url = url;
        }

        internal DiscordGame(JsonElement json)
        {
            Name = json.GetProperty("name").GetString()!;
            Type = (DiscordGameType)json.GetProperty("type").GetInt32();
            Url = json.GetPropertyOrNull("url")?.GetString();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
