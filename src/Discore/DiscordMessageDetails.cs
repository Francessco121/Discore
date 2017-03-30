namespace Discore
{
    /// <summary>
    /// Represents the details for an uncreated message.
    /// </summary>
    public class DiscordMessageDetails
    {
        /// <summary>
        /// Gets or sets the contents of the message.
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// Gets or sets whether the message should use text-to-speech.
        /// Default: false
        /// </summary>
        public bool TextToSpeech { get; set; }
        /// <summary>
        /// Gets or sets a nonce used for validating whether a message was created.
        /// </summary>
        public Snowflake? Nonce { get; set; }
        /// <summary>
        /// Gets or sets an embed to be sent with the message.
        /// </summary>
        public DiscordEmbedBuilder Embed { get; set; }

        public DiscordMessageDetails() { }

        public DiscordMessageDetails(string content)
        {
            Content = content;
        }

        /// <summary>
        /// Sets the contents of the message.
        /// </summary>
        public DiscordMessageDetails SetContent(string content)
        {
            Content = content;
            return this;
        }

        /// <summary>
        /// Sets whether the message should use text-to-speech.
        /// </summary>
        public DiscordMessageDetails SetTextToSpeech(bool useTextToSpeech)
        {
            TextToSpeech = useTextToSpeech;
            return this;
        }

        /// <summary>
        /// Sets a nonce used for validating whether a message was created.
        /// </summary>
        public DiscordMessageDetails SetNonce(Snowflake? nonce)
        {
            Nonce = nonce;
            return this;
        }

        /// <summary>
        /// Sets an embed to be sent with the message.
        /// </summary>
        public DiscordMessageDetails SetEmbed(DiscordEmbedBuilder embed)
        {
            Embed = embed;
            return this;
        }
    }
}
