using System.Collections.Generic;
using System.Text.Json;

namespace Discore.Http
{
    public class EditMessageOptions
    {
        /// <summary>
        /// Gets or sets the content of the message.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the embeds within the message.
        /// </summary>
        public IList<EmbedOptions>? Embeds { get; set; }

        /// <summary>
        /// Gets or sets the allowed mentions for the message.
        /// </summary>
        public AllowedMentionsOptions? AllowedMentions { get; set; }

        /// <summary>
        /// Gets or sets file attachments to keep or upload with the message.
        /// </summary>
        public IList<AttachmentOptions>? Attachments { get; set; }

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
        /// Sets the embeds within the message.
        /// </summary>
        public EditMessageOptions SetEmbeds(IList<EmbedOptions>? embeds)
        {
            Embeds = embeds;
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
        /// Sets attachments to keep or upload with the message.
        /// </summary>
        public EditMessageOptions SetAttachments(IList<AttachmentOptions>? attachments)
        {
            Attachments = attachments;
            return this;
        }

        /// <summary>
        /// Adds a new attachment or modifies an existing one.
        /// </summary>
        public EditMessageOptions AddOrSetAttachment(AttachmentOptions attachment)
        {
            Attachments ??= new List<AttachmentOptions>();
            Attachments.Add(attachment);
            return this;
        }

        /// <summary>
        /// Specifies that an existing attachment with the given ID should not be deleted when
        /// the message is edited.
        /// </summary>
        public EditMessageOptions KeepAttachment(Snowflake id)
        {
            Attachments ??= new List<AttachmentOptions>();
            Attachments.Add(new AttachmentOptions(id));
            return this;
        }

        /// <summary>
        /// Specifies that existing attachments should not be deleted when the message is edited.
        /// </summary>
        public EditMessageOptions KeepAttachments(IEnumerable<DiscordAttachment> attachments)
        {
            Attachments ??= new List<AttachmentOptions>();

            foreach (DiscordAttachment attachment in attachments)
                Attachments.Add(new AttachmentOptions(attachment.Id));

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

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteString("content", Content);

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
