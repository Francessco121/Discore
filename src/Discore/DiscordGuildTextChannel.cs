namespace Discore
{
    public sealed class DiscordGuildTextChannel : DiscordGuildChannel
    {
        /// <summary>
        /// Gets the topic of this channel.
        /// </summary>
        public string Topic { get; private set; }

        /// <summary>
        /// Gets the id of the last message sent in this channel.
        /// </summary>
        public string LastMessageId { get; private set; }

        internal DiscordGuildTextChannel(Shard shard, DiscordGuild guild) 
            : base(shard, guild, DiscordGuildChannelType.Text)
        { }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            Topic = data.GetString("topic") ?? Topic;
            LastMessageId = data.GetString("last_message_id") ?? LastMessageId;
        }
    }
}
