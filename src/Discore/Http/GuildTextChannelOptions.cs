namespace Discore.Http
{
    /// <summary>
    /// An optional set of parameters for modifying a guild text channel.
    /// </summary>
    public class GuildTextChannelOptions
    {
        /// <summary>
        /// Gets or sets the name of the channel (or null to leave unchanged).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the sorting position of the channel (or null to leave unchanged).
        /// </summary>
        public int? Position { get; set; }

        /// <summary>
        /// Gets or sets the topic of the text channel (or null to leave unchanged).
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// Sets the name of the channel.
        /// </summary>
        public GuildTextChannelOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the sorting position of the channel.
        /// </summary>
        public GuildTextChannelOptions SetPosition(int position)
        {
            Position = position;
            return this;
        }

        /// <summary>
        /// Sets the topic of the text channel.
        /// </summary>
        public GuildTextChannelOptions SetTopic(string topic)
        {
            Topic = topic;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);

            if (Name != null)
                data.Set("name", Name);
            if (Position.HasValue)
                data.Set("position", Position.Value);
            if (Topic != null)
                data.Set("topic", Topic);

            return data;
        }
    }
}
