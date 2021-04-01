#nullable enable

namespace Discore.WebSocket
{
    class MutableDMChannel : MutableEntity<DiscordDMChannel>
    {
        public Snowflake Id { get; }
        public MutableUser Recipient { get; }
        public Snowflake? LastMessageId { get; set; }

        public MutableDMChannel(Snowflake id, MutableUser recipient) 
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

#nullable restore
