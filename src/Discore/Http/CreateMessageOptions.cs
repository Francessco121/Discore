#nullable enable

using System.Text.Json;

namespace Discore.Http
{
    public class CreateMessageOptions
    {
        /// <summary>
        /// Gets or sets the contents of the message.
        /// </summary>
        public string? Content { get; set; }
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
        public EmbedOptions? Embed { get; set; }

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

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteString("content", Content);
            writer.WriteBoolean("tts", TextToSpeech);
            writer.WriteSnowflake("nonce", Nonce);

            if (Embed != null)
            {
                writer.WritePropertyName("embed");
                Embed.Build(writer);
            }

            writer.WriteEndObject();
        }
    }
}

#nullable restore
