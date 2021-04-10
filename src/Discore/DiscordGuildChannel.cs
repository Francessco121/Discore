using System.Collections.Generic;
using System.Text.Json;

#nullable enable

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

        protected DiscordGuildChannel(
            Snowflake id,
            DiscordChannelType type,
            string name,
            int position,
            IReadOnlyDictionary<Snowflake, DiscordOverwrite> permissionOverwrites,
            Snowflake guildId)
            : base(id, type)
        {
            Name = name;
            Position = position;
            PermissionOverwrites = permissionOverwrites;
            GuildId = guildId;
        }

        internal DiscordGuildChannel(JsonElement json, DiscordChannelType type, Snowflake? guildId)
            : base(json, type)
        {
            GuildId = guildId ?? json.GetProperty("guild_id").GetSnowflake();
            Name = json.GetProperty("name").GetString()!;
            Position = json.GetProperty("position").GetInt32();

            JsonElement? overwritesJson = json.GetPropertyOrNull("permission_overwrites");
            if (overwritesJson != null)
            {
                JsonElement _overwritesJson = overwritesJson.Value;
                var overwrites = new Dictionary<Snowflake, DiscordOverwrite>();

                int numOverwrites = _overwritesJson.GetArrayLength();
                for (int i = 0; i < numOverwrites; i++)
                {
                    var overwrite = new DiscordOverwrite(_overwritesJson[i], channelId: Id);
                    overwrites[overwrite.Id] = overwrite;
                }

                PermissionOverwrites = overwrites;
            }
            else
            {
                PermissionOverwrites = new Dictionary<Snowflake, DiscordOverwrite>();
            }
        }

        public override string ToString()
        {
            return $"{ChannelType} Channel: {Name}";
        }
    }
}

#nullable restore
