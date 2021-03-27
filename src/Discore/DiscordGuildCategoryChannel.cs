namespace Discore
{
    public sealed class DiscordGuildCategoryChannel : DiscordGuildChannel
    {
        internal DiscordGuildCategoryChannel(DiscordApiData data, Snowflake? guildId = null)
            : base(data, DiscordChannelType.GuildCategory, guildId)
        { }
    }
}
