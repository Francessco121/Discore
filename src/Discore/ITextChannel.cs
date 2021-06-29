namespace Discore
{
    public interface ITextChannel
    {
        /// <summary>
        /// Gets the ID of this text channel.
        /// </summary>
        Snowflake Id { get; }
        /// <summary>
        /// Gets the type of this channel.
        /// </summary>
        DiscordChannelType ChannelType { get; }
        /// <summary>
        /// Gets the ID of the last message sent in this text channel.
        /// <para/>
        /// This ID is only up-to-date for when this text channel was first retrieved from the Discord API.
        /// It's very likely that this value is outdated.
        /// <para/>
        /// Use <see cref="Http.DiscordHttpClient.GetChannel{T}(Snowflake)"/> to get an up-to-date ID.
        /// </summary>
        Snowflake? LastMessageId { get; }
    }
}
