using System.Text.Json;

namespace Discore.Http
{
    public class ModifyGuildEmojiOptions
    {
        /// <summary>
        /// Gets or sets the name of the emoji (or null to leave unchanged).
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Sets the name of the emoji.
        /// </summary>
        public ModifyGuildEmojiOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            if (Name != null)
                writer.WriteString("name", Name);

            writer.WriteEndObject();
        }
    }
}
