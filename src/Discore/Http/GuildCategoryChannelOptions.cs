namespace Discore.Http
{
    /// <summary>
    /// An optional set of parameters for modifying a guild category channel.
    /// </summary>
    public class GuildCategoryChannelOptions
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
        /// Sets the name of the channel.
        /// </summary>
        public GuildCategoryChannelOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the sorting position of the channel.
        /// </summary>
        public GuildCategoryChannelOptions SetPosition(int position)
        {
            Position = position;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);

            if (Name != null)
                data.Set("name", Name);
            if (Position.HasValue)
                data.Set("position", Position.Value);

            return data;
        }
    }
}
