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

        /// <summary>
        /// Gets or sets the allowed mentions for the message.
        /// </summary>
        public AllowedMentionsOptions AllowedMentions { get; set; }

        /// <summary>
        /// Gets or sets the flags of the message.
        /// <para/>
        /// Note: Only <see cref="DiscordMessageFlags.SuppressEmbeds"/> can be set/unset.
        /// </summary>
        public DiscordMessageFlags? Flags { get; set; }

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

        /// <summary>
        /// Sets which mentions are allowed for the message.
        /// </summary>
        public EditMessageOptions SetAllowedMentions(AllowedMentionsOptions allowedMentions)
        {
            AllowedMentions = allowedMentions;
            return this;
        }

        /// <summary>
        /// Sets the flags of the message.
        /// <para/>
        /// Note: Only <see cref="DiscordMessageFlags.SuppressEmbeds"/> can be set/unset.
        /// </summary>
        public EditMessageOptions SetFlags(DiscordMessageFlags? flags)
        {
            Flags = flags;
            return this;
        }
    }
}
