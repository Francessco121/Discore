#nullable enable

using System.Collections.Generic;
using System.Text.Json;

namespace Discore
{
    public sealed class DiscordGuildCategoryChannel : DiscordGuildChannel
    {
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
