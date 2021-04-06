#nullable enable

using System.Text.Json;

namespace Discore.Http
{
    public class CreateGuildEmojiOptions
    {
        /// <summary>
        /// Gets or sets the name of the emoji.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the emoji's image.
        /// </summary>
        public DiscordImageData? Image { get; set; }

        /// <summary>
        /// Sets the name of the emoji.
        /// </summary>
        public CreateGuildEmojiOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the emoji's image.
        /// </summary>
        public CreateGuildEmojiOptions SetImage(DiscordImageData image)
        {
            Image = image;
            return this;
        }

        internal void Build(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WriteString("name", Name);
            writer.WriteString("image", Image?.ToDataUriScheme());

            writer.WriteEndObject();
        }
    }
}

#nullable restore
