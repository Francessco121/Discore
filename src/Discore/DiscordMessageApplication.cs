namespace Discore
{
    public class DiscordMessageApplication : DiscordIdEntity
    {
        /// <summary>
        /// Gets the ID of the embed's image asset.
        /// May be null.
        /// </summary>
        public string CoverImage { get; }
        /// <summary>
        /// Gets the description of the application.
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Gets the ID of the application's icon.
        /// May be null.
        /// </summary>
        public string Icon { get; }
        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        public string Name { get; }

        internal DiscordMessageApplication(DiscordApiData data)
            : base(data)
        {
            CoverImage = data.GetString("cover_image");
            Description = data.GetString("description");
            Icon = data.GetString("icon");
            Name = data.GetString("name");
        }
    }
}
