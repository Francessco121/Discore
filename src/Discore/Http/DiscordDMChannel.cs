namespace Discore.Http
{
    /// <summary>
    /// Direct message channels represent a one-to-one conversation between two users, outside of the scope of guilds.
    /// </summary>
    public class DiscordDMChannel : DiscordChannel
    {
        /// <summary>
        /// The id of the last message sent in this DM.
        /// </summary>
        public string LastMessageId { get; }

        /// <summary>
        /// Gets the user on the other end of this channel.
        /// </summary>
        public DiscordUser Recipient { get; }

        public DiscordDMChannel(DiscordApiData data)
            : base(data, DiscordChannelType.DirectMessage)
        {
            DiscordApiData recipientData = data.Get("recipient");
            Recipient = new DiscordUser(recipientData);

            LastMessageId = data.GetString("last_message_id");
        }

        public override string ToString()
        {
            return $"DM Channel: {Recipient}";
        }
    }
}
