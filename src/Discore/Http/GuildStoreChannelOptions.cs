using System.Collections.Generic;

namespace Discore.Http
{
    /// <summary>
    /// An optional set of parameters for modifying a guild store channel.
    /// </summary>
    public class GuildStoreChannelOptions
    {
        /// <summary>
        /// Gets or sets the name of the channel (or null to leave unchanged).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the sorting position of the channel (or null to leave unchanged).
        /// </summary>
        public int? Position { get; set; }

        /// <summary>
        /// Gets or sets whether this store channel is NSFW (not-safe-for-work) (or null to leave unchanged).
        /// </summary>
        public bool? Nsfw { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent category channel (or null to leave unchanged).
        /// <para>Note: Set to <see cref="Snowflake.None"/> to clear the parent ID.</para>
        /// </summary>
        public Snowflake? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the list of permission overwrites (or null to leave unchanged).
        /// </summary>
        public IList<OverwriteOptions> PermissionOverwrites { get; set; }

        /// <summary>
        /// Sets the name of the channel.
        /// </summary>
        public GuildStoreChannelOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the sorting position of the channel.
        /// </summary>
        public GuildStoreChannelOptions SetPosition(int position)
        {
            Position = position;
            return this;
        }

        /// <summary>
        /// Sets whether this store channel is NSFW (not-safe-for-work).
        /// </summary>
        public GuildStoreChannelOptions SetNsfw(bool nsfw)
        {
            Nsfw = nsfw;
            return this;
        }

        /// <summary>
        /// Sets the ID of the parent category channel.
        /// </summary>
        /// <param name="parentId">
        /// The ID of the category to use as a parent or <see cref="Snowflake.None"/> to clear the parent ID.
        /// </param>
        public GuildStoreChannelOptions SetParentId(Snowflake parentId)
        {
            ParentId = parentId;
            return this;
        }

        /// <summary>
        /// Sets the list of permission overwrites.
        /// </summary>
        public GuildStoreChannelOptions SetPermissionOverwrites(IList<OverwriteOptions> permissionOverwrites)
        {
            PermissionOverwrites = permissionOverwrites;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);

            if (Name != null)
                data.Set("name", Name);
            if (Position.HasValue)
                data.Set("position", Position.Value);
            if (Nsfw.HasValue)
                data.Set("nsfw", Nsfw.Value);

            if (ParentId.HasValue)
            {
                if (ParentId.Value == Snowflake.None)
                    data.SetSnowflake("parent_id", null);
                else
                    data.SetSnowflake("parent_id", ParentId.Value);
            }

            if (PermissionOverwrites != null)
            {
                DiscordApiData permissionOverwritesArray = new DiscordApiData(DiscordApiDataType.Array);
                foreach (OverwriteOptions overwriteParam in PermissionOverwrites)
                    permissionOverwritesArray.Values.Add(overwriteParam.Build());

                data.Set("permission_overwrites", permissionOverwritesArray);
            }

            return data;
        }
    }
}
