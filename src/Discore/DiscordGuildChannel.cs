using System.Collections.Generic;

namespace Discore
{
    public abstract class DiscordGuildChannel : DiscordChannel
    {
        /// <summary>
        /// Gets the name of this channel.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the UI ordering position of this channel.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Gets a dictionary of all permission overwrites associated with this channel.
        /// </summary>
        public IReadOnlyDictionary<Snowflake, DiscordOverwrite> PermissionOverwrites { get; }

        /// <summary>
        /// Gets the ID of the guild this channel is in.
        /// </summary>
        public Snowflake GuildId { get; }

        internal DiscordGuildChannel(DiscordApiData data, DiscordChannelType type, Snowflake? guildId) 
            : base(data, type)
        {
            GuildId = guildId ?? data.GetSnowflake("guild_id").Value;
            Name = data.GetString("name");
            Position = data.GetInteger("position").Value;

            IList<DiscordApiData> overwrites = data.GetArray("permission_overwrites");
            Dictionary<Snowflake, DiscordOverwrite> permissionOverwrites = new Dictionary<Snowflake, DiscordOverwrite>();

            for (int i = 0; i < overwrites.Count; i++)
            {
                DiscordOverwrite overwrite = new DiscordOverwrite(Id, overwrites[i]);
                permissionOverwrites.Add(overwrite.Id, overwrite);
            }

            PermissionOverwrites = permissionOverwrites;
        }

        public override string ToString()
        {
            return $"{ChannelType} Channel: {Name}";
        }
    }
}
