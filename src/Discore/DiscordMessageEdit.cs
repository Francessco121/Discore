namespace Discore
{
    /// <summary>
    /// Represents changes to be made to a message.
    /// </summary>
    public class DiscordMessageEdit
    {
        /// <summary>
        /// Gets or sets the content of the message.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the embed within the message.
        /// </summary>
        public DiscordEmbedBuilder Embed { get; set; }

        public DiscordMessageEdit() { }

        public DiscordMessageEdit(string content)
        {
            Content = content;
        }

        /// <summary>
        /// Sets the content of the message.
        /// </summary>
        public DiscordMessageEdit SetContent(string content)
        {
            Content = content;
            return this;
        }

        /// <summary>
        /// Sets the embed within the message.
        /// </summary>
        public DiscordMessageEdit SetEmbed(DiscordEmbedBuilder embed)
        {
            Embed = embed;
            return this;
        }
    }
}
