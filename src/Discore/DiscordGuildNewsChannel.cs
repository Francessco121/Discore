namespace Discore
{
    public sealed class DiscordGuildNewsChannel : DiscordGuildChannel, ITextChannel
    {
        /// <summary>
        /// Gets the topic of this channel.
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// Gets whether this news channel is NSFW (not-safe-for-work).
        /// </summary>
        public bool Nsfw { get; }

        /// <summary>
        /// Gets the ID of the parent category channel or null if the channel is not in a category.
        /// </summary>
        public Snowflake? ParentId { get; }

        /// <summary>
        /// Gets the ID of the last message sent in this text channel.
        /// <para/>
        /// This ID is only up-to-date for when this text channel was first retrieved from the Discord API.
        /// It's very likely that this value is outdated.
        /// <para/>
        /// Use <see cref="Http.DiscordHttpClient.GetChannel{T}(Snowflake)"/> to get an up-to-date ID.
        /// </summary>
        public Snowflake LastMessageId { get; }

        internal DiscordGuildNewsChannel(DiscordApiData data, Snowflake? guildId = null) 
            : base(data, DiscordChannelType.GuildNews, guildId)
        {
            Topic = data.GetString("topic");
            Nsfw = data.GetBoolean("nsfw") ?? false;
            ParentId = data.GetSnowflake("parent_id");
            LastMessageId = data.GetSnowflake("last_message_id") ?? default(Snowflake);
        }
    }
}
