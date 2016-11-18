namespace Discore.Http
{
    public class DiscordGuildTextChannel : DiscordGuildChannel
    {
        /// <summary>
        /// Gets the topic of this channel.
        /// </summary>
        public string Topic { get; }

        /// <summary>
        /// Gets the id of the last message sent in this channel.
        /// </summary>
        public string LastMessageId { get; }

        public DiscordGuildTextChannel(DiscordApiData data)
            : base(data, DiscordGuildChannelType.Text)
        {
            Topic = data.GetString("topic");
            LastMessageId = data.GetString("last_message_id");
        }
    }
}
