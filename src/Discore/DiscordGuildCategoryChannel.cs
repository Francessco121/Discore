using System.Text.Json;

namespace Discore
{
    public class DiscordGuildCategoryChannel : DiscordGuildChannel
    {
        internal DiscordGuildCategoryChannel(JsonElement json, Snowflake? guildId = null)
            : base(json, DiscordChannelType.GuildCategory, guildId)
        { }
    }
}
