using Discore.Http;

namespace Discore.WebSocket
{
    class MutableDMChannel : MutableEntity<DiscordDMChannel>
    {
        public Snowflake Id { get; }
        public MutableUser Recipient { get; }
        public Snowflake LastMessageId { get; set; }

        public MutableDMChannel(Snowflake id, MutableUser recipient, DiscordHttpClient http) 
            : base(http)
        {
            Id = id;

            Recipient = recipient;
            Reference(recipient);
        }

        protected override DiscordDMChannel BuildImmutableEntity()
        {
            return new DiscordDMChannel(Id, Recipient.ImmutableEntity, LastMessageId);
        }
    }
}
