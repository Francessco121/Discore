using System.Collections.Generic;

namespace Discore.Http
{
    /// <summary>
    /// An optional set of parameters for modifying a guild category channel.
    /// </summary>
    public class GuildCategoryChannelOptions
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
        /// Gets or sets the list of permission overwrites (or null to leave unchanged).
        /// </summary>
        public IList<OverwriteOptions> PermissionOverwrites { get; set; }

        /// <summary>
        /// Sets the name of the channel.
        /// </summary>
        public GuildCategoryChannelOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the sorting position of the channel.
        /// </summary>
        public GuildCategoryChannelOptions SetPosition(int position)
        {
            Position = position;
            return this;
        }

        /// <summary>
        /// Sets the list of permission overwrites.
        /// </summary>
        public GuildCategoryChannelOptions SetPermissionOverwrites(IList<OverwriteOptions> permissionOverwrites)
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
