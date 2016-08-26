namespace Discore
{
    public class DiscordDMChannel : DiscordChannel
    {
        public bool IsPrivate { get; private set; }
        public DiscordUser Recipient { get; private set; }
        public string LastMessageId { get; private set; }

        public DiscordDMChannel(IDiscordClient client) 
            : base(client, DiscordChannelType.DirectMessage)
        { }

        public override void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            IsPrivate = data.GetBoolean("is_private") ?? IsPrivate;
            LastMessageId = data.GetString("last_message_id") ?? LastMessageId;

            DiscordApiData recipientData = data.Get("recipient");
            if (recipientData != null)
            {
                string userId = recipientData.GetString("id");
                Cache.AddOrUpdate(userId, recipientData, () => { return new DiscordUser(); });
            }
        }
    }
}
