using System.Collections.Generic;

namespace Discore.Http
{
    public abstract class DiscordGuildChannel : DiscordChannel
    {
        public DiscordGuildChannelType GuildChannelType { get; }

        /// <summary>
        /// Gets the name of this channel.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the UI ordering position of this channel.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Gets a list of all permission overwrites associated with this channel.
        /// </summary>
        public DiscordOverwrite[] PermissionOverwrites { get; }

        /// <summary>
        /// Gets the id of the guild this channel is in.
        /// </summary>
        public Snowflake GuildId { get; }

        internal DiscordGuildChannel(DiscordApiData data, DiscordGuildChannelType type) 
            : base(data, DiscordChannelType.Guild)
        {
            GuildChannelType = type;

            GuildId = data.GetSnowflake("guild_id").Value;
            Name = data.GetString("name");
            Position = data.GetInteger("position").Value;

            IList<DiscordApiData> overwrites = data.GetArray("permission_overwrites");
            PermissionOverwrites = new DiscordOverwrite[overwrites.Count];

            for (int i = 0; i < overwrites.Count; i++)
                PermissionOverwrites[i] = new DiscordOverwrite(overwrites[i]);
        }

        public override string ToString()
        {
            return $"{GuildChannelType} Channel: {Name}";
        }
    }
}
