#nullable enable

using System.Text.Json;

namespace Discore
{
    public class DiscordMessageApplication : DiscordIdEntity
    {
        /// <summary>
        /// Gets the ID of the embed's image asset.
        /// May be null.
        /// </summary>
        public string? CoverImage { get; }
        /// <summary>
        /// Gets the description of the application.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Gets the ID of the application's icon.
        /// May be null.
        /// </summary>
        public string? Icon { get; }
        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        public string Name { get; }

        public DiscordMessageApplication(
            Snowflake id,
            string? coverImage, 
            string description, 
            string? icon, 
            string name)
            : base(id)
        {
            CoverImage = coverImage;
            Description = description;
            Icon = icon;
            Name = name;
        }

        internal DiscordMessageApplication(JsonElement json)
            : base(json)
        {
            CoverImage = json.GetPropertyOrNull("cover_image")?.GetString();
            Description = json.GetProperty("description").GetString()!;
            Icon = json.GetPropertyOrNull("icon")?.GetString();
            Name = json.GetProperty("name").GetString()!;
        }
    }
}

#nullable restore
