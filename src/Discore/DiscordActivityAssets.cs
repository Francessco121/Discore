namespace Discore
{
    public class DiscordActivityAssets
    {
        /// <summary>
        /// Gets the ID for a large asset of the activity, usually a snowflake. May be null.
        /// </summary>
        public string LargeImage { get; }

        /// <summary>
        /// Gets the text displayed when hovering over the large image of the activity. May be null.
        /// </summary>
        public string LargeText { get; }

        /// <summary>
        /// Gets the ID for a small asset of the activity, usually a snowflake. May be null.
        /// </summary>
        public string SmallImage { get; }

        /// <summary>
        /// Gets the text displayed when hovering over the small image of the activity. May be null.
        /// </summary>
        public string SmallText { get; }

        internal DiscordActivityAssets(DiscordApiData data)
        {
            LargeImage = data.GetString("large_image");
            LargeText = data.GetString("large_text");
            SmallImage = data.GetString("small_image");
            SmallText = data.GetString("small_text");
        }
    }
}
