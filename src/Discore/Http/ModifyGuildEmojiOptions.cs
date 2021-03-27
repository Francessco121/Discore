namespace Discore.Http
{
    public class ModifyGuildEmojiOptions
    {
        /// <summary>
        /// Gets or sets the name of the emoji (or null to leave unchanged).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sets the name of the emoji.
        /// </summary>
        public ModifyGuildEmojiOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);

            if (Name != null)
                data.Set("name", Name);

            return data;
        }
    }
}
