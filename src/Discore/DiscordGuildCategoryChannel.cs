#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Discore
{
    public sealed class DiscordGuildCategoryChannel : DiscordGuildChannel
    {
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="name"/> or <paramref name="permissionOverwrites"/> is null.
        /// </exception>
        public DiscordGuildCategoryChannel(
            Snowflake id, 
            string name, 
            int position, 
            IReadOnlyDictionary<Snowflake, DiscordOverwrite> permissionOverwrites, 
            Snowflake guildId) 
            : base(id, 
                  DiscordChannelType.GuildCategory, 
                  name, 
                  position, 
                  permissionOverwrites, 
                  guildId)
        { }

        internal DiscordGuildCategoryChannel(JsonElement json, Snowflake? guildId = null)
            : base(json, DiscordChannelType.GuildCategory, guildId)
        { }
    }
}

#nullable restore
