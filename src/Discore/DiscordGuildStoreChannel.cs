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

        internal DiscordGuildStoreChannel(DiscordApiData data, Snowflake? guildId = null) 
            : base(data, DiscordChannelType.GuildStore, guildId)
        {
            Nsfw = data.GetBoolean("nsfw") ?? false;
            ParentId = data.GetSnowflake("parent_id");
        }
    }
}
