using System.Collections.Generic;
using System.Text.Json;

#nullable enable

namespace Discore
{
    public sealed class DiscordGuildStoreChannel : DiscordGuildChannel
    {
        /// <summary>
        /// Gets whether this store channel is NSFW (not-safe-for-work).
        /// </summary>
        public bool Nsfw { get; }

        /// <summary>
        /// Gets the ID of the parent category channel or null if the channel is not in a category.
        /// </summary>
        public Snowflake? ParentId { get; }

        public DiscordGuildStoreChannel(
            Snowflake id,
            string name, 
            int position, 
            IReadOnlyDictionary<Snowflake, DiscordOverwrite> permissionOverwrites, 
            Snowflake guildId,
            bool nsfw,
            Snowflake? parentId) 
            : base(id,
                  DiscordChannelType.GuildStore, 
                  name, 
                  position, 
                  permissionOverwrites, 
                  guildId)
        {
            Nsfw = nsfw;
            ParentId = parentId;
        }

        internal DiscordGuildStoreChannel(JsonElement json, Snowflake? guildId = null)
            : base(json, DiscordChannelType.GuildStore, guildId)
        {
            Nsfw = json.GetProperty("nsfw").GetBoolean();
            ParentId = json.GetProperty("parent_id").GetSnowflakeOrNull();
        }
    }
}

#nullable restore
