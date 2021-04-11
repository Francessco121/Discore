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
        /// Gets or sets the embed within the message.
        /// </summary>
        public EmbedOptions? Embed { get; set; }

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

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteString("content", Content);

            if (Embed != null)
            {
                writer.WritePropertyName("embed");
                Embed.Build(writer);
            }

            writer.WriteEndObject();
        }
    }
}
