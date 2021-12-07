namespace Discore.Http
{
    public class MessageReferenceOptions
    {
        /// <summary>
        /// Gets or sets the ID of the message to reference.
        /// </summary>
        public Snowflake MessageId { get; set; }

        /// <summary>
        /// Gets or sets whether to error if the referenced message doesn't exist instead
        /// of sending as a normal (non-reply) message.
        /// <para/>
        /// Defaults to true if null is given.
        /// </summary>
        public bool? FailIfNotExists { get; set; }

        public MessageReferenceOptions() { }
     
        public MessageReferenceOptions(Snowflake messageId)
        {
            MessageId = messageId;
        }

        /// <summary>
        /// Sets the ID of the message to reference.
        /// </summary>
        public MessageReferenceOptions SetMessageId(Snowflake messageId)
        {
            MessageId = messageId;
            return this;
        }

        /// <summary>
        /// Sets whether to error if the referenced message doesn't exist instead
        /// of sending as a normal (non-reply) message.
        /// </summary>
        public MessageReferenceOptions SetFailIfNotExists(bool? failIfNotExists)
        {
            FailIfNotExists = failIfNotExists;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.SetSnowflake("message_id", MessageId);
            
            if (FailIfNotExists != null)
                data.Set("fail_if_not_exists", FailIfNotExists.Value);

            return data;
        }
    }
}
