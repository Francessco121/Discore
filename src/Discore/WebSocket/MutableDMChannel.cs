using Discore.Http;

namespace Discore.WebSocket
{
    class MutableDMChannel : MutableEntity<DiscordDMChannel>
    {
        public Snowflake Id { get; }
        public MutableUser Recipient { get; }
        public Snowflake LastMessageId { get; set; }

        public MutableDMChannel(Snowflake id, MutableUser recipient, DiscordHttpApi http) 
            : base(http)
        {
            Id = id;

            Recipient = recipient;
            Reference(recipient);
        }

        public void Update(DiscordApiData data)
        {
            LastMessageId = data.GetSnowflake("last_message_id") ?? default(Snowflake);

            Dirty();
        }

        protected override DiscordDMChannel BuildImmutableEntity()
        {
            return new DiscordDMChannel(Http, this);
        }
    }
}
