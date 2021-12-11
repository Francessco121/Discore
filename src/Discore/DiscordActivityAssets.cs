using System.Text.Json;

namespace Discore
{
    public class DiscordActivityAssets
    {
        /// <summary>
        /// Gets the ID for a large asset of the activity, usually a snowflake. May be null.
        /// </summary>
        public string? LargeImage { get; }

        /// <summary>
        /// Gets the text displayed when hovering over the large image of the activity. May be null.
        /// </summary>
        public string? LargeText { get; }

        /// <summary>
        /// Gets the ID for a small asset of the activity, usually a snowflake. May be null.
        /// </summary>
        public string? SmallImage { get; }

        /// <summary>
        /// Gets the text displayed when hovering over the small image of the activity. May be null.
        /// </summary>
        public string? SmallText { get; }

        internal DiscordActivityAssets(JsonElement json)
        {
            LargeImage = json.GetPropertyOrNull("large_image")?.GetString();
            LargeText = json.GetPropertyOrNull("large_text")?.GetString();
            SmallImage = json.GetPropertyOrNull("small_image")?.GetString();
            SmallText = json.GetPropertyOrNull("small_text")?.GetString();
        }
    }
}
