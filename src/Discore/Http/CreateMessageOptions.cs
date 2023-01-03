using System.Collections.Generic;
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
        /// Gets or sets embeds to be sent with the message.
        /// </summary>
        public IList<EmbedOptions>? Embeds { get; set; }

        /// <summary>
        /// Gets or sets the allowed mentions for the message.
        /// </summary>
        public AllowedMentionsOptions? AllowedMentions { get; set; }

        /// <summary>
        /// Gets or sets the message to reply to.
        /// </summary>
        public MessageReferenceOptions? MessageReference { get; set; }

        /// <summary>
        /// Gets or sets file attachments to upload with the message.
        /// </summary>
        public IList<AttachmentOptions>? Attachments { get; set; }

        /// <summary>
        /// Gets or sets the flags of the message.
        /// <para/>
        /// Note: Only <see cref="DiscordMessageFlags.SuppressEmbeds"/> can be set/unset.
        /// </summary>
        public DiscordMessageFlags? Flags { get; set; }

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
        /// Sets embeds to be sent with the message.
        /// </summary>
        public CreateMessageOptions SetEmbeds(IList<EmbedOptions>? embeds)
        {
            Embeds = embeds;
            return this;
        }

        /// <summary>
        /// Adds an embed to be sent with the message.
        /// </summary>
        public CreateMessageOptions AddEmbed(EmbedOptions embed)
        {
            Embeds ??= new List<EmbedOptions>();
            Embeds.Add(embed);
            return this;
        }

        /// <summary>
        /// Sets which mentions are allowed for the message.
        /// </summary>
        public CreateMessageOptions SetAllowedMentions(AllowedMentionsOptions? allowedMentions)
        {
            AllowedMentions = allowedMentions;
            return this;
        }

        /// <summary>
        /// Sets the message to reply to.
        /// </summary>
        public CreateMessageOptions SetMessageReference(MessageReferenceOptions? messageReference)
        {
            MessageReference = messageReference;
            return this;
        }

        /// <summary>
        /// Sets attachments to upload with the message.
        /// </summary>
        public CreateMessageOptions SetAttachments(IList<AttachmentOptions>? attachments)
        {
            Attachments = attachments;
            return this;
        }

        /// <summary>
        /// Adds an attachment to upload with the message.
        /// </summary>
        public CreateMessageOptions AddAttachment(AttachmentOptions attachment)
        {
            Attachments ??= new List<AttachmentOptions>();
            Attachments.Add(attachment);
            return this;
        }

        /// <summary>
        /// Sets the flags of the message.
        /// <para/>
        /// Note: Only <see cref="DiscordMessageFlags.SuppressEmbeds"/> can be set/unset.
        /// </summary>
        public CreateMessageOptions SetFlags(DiscordMessageFlags? flags)
        {
            Flags = flags;
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteString("content", Content);
            writer.WriteBoolean("tts", TextToSpeech);
            writer.WriteSnowflake("nonce", Nonce);

            if (Embeds != null)
            {
                writer.WriteStartArray("embeds");

                foreach (EmbedOptions embed in Embeds)
                    embed.Build(writer);

                writer.WriteEndArray();
            }

            if (AllowedMentions != null)
            {
                writer.WritePropertyName("allowed_mentions");
                AllowedMentions.Build(writer);
            }

            if (MessageReference != null)
            {
                writer.WritePropertyName("message_reference");
                MessageReference.Build(writer);
            }

            if (Attachments != null)
            {
                writer.WriteStartArray("attachments");

                foreach (AttachmentOptions attachment in Attachments)
                    attachment.Build(writer);

                writer.WriteEndArray();
            }

            if (Flags != null)
                writer.WriteNumber("flags", (int)Flags.Value);

            writer.WriteEndObject();
        }
    }
}
