using System.Text.Json;

namespace Discore
{
    /// <summary>
    /// Represents an activity a user is currently engaged in.
    /// </summary>
    public class DiscordActivity
    {
        /// <summary>
        /// Gets the name of the activity.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the type of activity.
        /// </summary>
        public DiscordActivityType Type { get; }
        /// <summary>
        /// Gets the URL of the stream when the type is set to "Streaming" and the URL is valid.
        /// Otherwise, returns null.
        /// </summary>
        public string? Url { get; }

        internal DiscordActivity(JsonElement json)
        {
            Name = json.GetProperty("name").GetString()!;
            Type = (DiscordActivityType)json.GetProperty("type").GetInt32();
            Url = json.GetPropertyOrNull("url")?.GetString();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
