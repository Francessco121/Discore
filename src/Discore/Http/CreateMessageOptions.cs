namespace Discore.Http
{
    public class CreateMessageOptions
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
        public EmbedOptions Embed { get; set; }
        /// <summary>
        /// Gets or sets the allowed mentions for the message.
        /// </summary>
        public AllowedMentionsOptions AllowedMentions { get; set; }
        /// <summary>
        /// Gets or sets the message to reply to.
        /// </summary>
        public MessageReferenceOptions MessageReference { get; set; }

        public CreateMessageOptions() { }

        public CreateMessageOptions(string content)
        {
            Content = content;
        }

        /// <summary>
        /// Sets the contents of the message.
        /// </summary>
        public CreateMessageOptions SetContent(string content)
        {
            Content = content;
            return this;
        }

        /// <summary>
        /// Sets whether the message should use text-to-speech.
        /// </summary>
        public CreateMessageOptions SetTextToSpeech(bool useTextToSpeech)
        {
            TextToSpeech = useTextToSpeech;
            return this;
        }

        /// <summary>
        /// Sets a nonce used for validating whether a message was created.
        /// </summary>
        public CreateMessageOptions SetNonce(Snowflake? nonce)
        {
            Nonce = nonce;
            return this;
        }

        /// <summary>
        /// Sets an embed to be sent with the message.
        /// </summary>
        public CreateMessageOptions SetEmbed(EmbedOptions embed)
        {
            Embed = embed;
            return this;
        }

        /// <summary>
        /// Sets which mentions are allowed for the message.
        /// </summary>
        public CreateMessageOptions SetAllowedMentions(AllowedMentionsOptions allowedMentions)
        {
            AllowedMentions = allowedMentions;
            return this;
        }

        /// <summary>
        /// Sets the message to reply to.
        /// </summary>
        public CreateMessageOptions SetMessageReference(MessageReferenceOptions messageReference)
        {
            MessageReference = messageReference;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("content", Content);
            data.Set("tts", TextToSpeech);
            data.SetSnowflake("nonce", Nonce);

            if (Embed != null)
                data.Set("embed", Embed.Build());

            if (AllowedMentions != null)
                data.Set("allowed_mentions", AllowedMentions.Build());

            if (MessageReference != null)
                data.Set("message_reference", MessageReference.Build());

            return data;
        }
    }
}
