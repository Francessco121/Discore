namespace Discore.Http
{
    public class EditMessageOptions
    {
        /// <summary>
        /// Gets or sets the content of the message.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the embed within the message.
        /// </summary>
        public EmbedOptions Embed { get; set; }

        public EditMessageOptions() { }

        public EditMessageOptions(string content)
        {
            Content = content;
        }

        /// <summary>
        /// Sets the content of the message.
        /// </summary>
        public EditMessageOptions SetContent(string content)
        {
            Content = content;
            return this;
        }

        /// <summary>
        /// Sets the embed within the message.
        /// </summary>
        public EditMessageOptions SetEmbed(EmbedOptions embed)
        {
            Embed = embed;
            return this;
        }
    }
}
