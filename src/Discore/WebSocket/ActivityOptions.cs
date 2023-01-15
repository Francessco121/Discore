namespace Discore.WebSocket
{
    public class ActivityOptions
    {
        /// <summary>
        /// The name of the activity.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The type of activity. Defaults to <see cref="DiscordActivityType.Game"/>.
        /// </summary>
        public DiscordActivityType Type { get; set; } = DiscordActivityType.Game;

        /// <summary>
        /// The URL of the stream. <see cref="Type"/> must be <see cref="DiscordActivityType.Streaming"/>
        /// for this to take effect. Defaults to null.
        /// </summary>
        public string? Url { get; set; }

        public ActivityOptions() { }
        public ActivityOptions(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Sets the name of the activity.
        /// </summary>
        public ActivityOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the type of activity.
        /// </summary>
        public ActivityOptions SetType(DiscordActivityType type)
        {
            Type = type;
            return this;
        }

        /// <summary>
        /// Sets the URL of the stream. <see cref="Type"/> must be <see cref="DiscordActivityType.Streaming"/>
        /// for this to take effect.
        /// </summary>
        public ActivityOptions SetUrl(string url)
        {
            Url = url;
            return this;
        }
    }
}
