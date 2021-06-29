using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Discore
{
    public class DiscordGuildStoreChannel : DiscordGuildChannel
    {
        /// <summary>
        /// Gets whether this store channel is NSFW (not-safe-for-work).
        /// </summary>
        public bool Nsfw { get; }

        /// <summary>
        /// Gets the ID of the parent category channel or null if the channel is not in a category.
        /// </summary>
        public Snowflake? ParentId { get; }

        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="name"/> or <paramref name="permissionOverwrites"/> is null.
        /// </exception>
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
            Nsfw = json.GetPropertyOrNull("nsfw")?.GetBoolean() ?? false;
            ParentId = json.GetPropertyOrNull("parent_id")?.GetSnowflakeOrNull();
        }
    }
}
