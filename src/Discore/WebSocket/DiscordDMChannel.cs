using Discore.Http.Net;

namespace Discore.WebSocket
{
    /// <summary>
    /// Direct message channels represent a one-to-one conversation between two users, outside of the scope of guilds.
    /// </summary>
    public sealed class DiscordDMChannel : DiscordChannel
    {
        /// <summary>
        /// The id of the last message sent in this DM.
        /// </summary>
        public string LastMessageId { get; private set; }

        /// <summary>
        /// Gets the user on the other end of this channel.
        /// </summary>
        public DiscordUser Recipient { get; private set; }

        Shard shard;
        HttpChannelsEndpoint channelsHttp;

        internal DiscordDMChannel(Shard shard) 
            : base(shard, DiscordChannelType.DirectMessage)
        {
            this.shard = shard;

            channelsHttp = shard.Application.InternalHttpApi.Channels;
        }

        /// <summary>
        /// Sends a message to this direct message channel.
        /// Returns the message sent, or null if the send failed.
        /// </summary>
        public DiscordMessage SendMessage(string content)
        {
            DiscordApiData data = channelsHttp.Create(Id, content);

            if (data != null && !data.IsNull)
            {
                DiscordMessage msg = new DiscordMessage(shard);
                msg.Update(data);

                return msg;
            }
            else
                return null;
        }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            DiscordApiData recipientData = data.Get("recipient");
            if (recipientData != null)
            {
                Snowflake recipientId = recipientData.GetSnowflake("id").Value;
                Recipient = shard.Users.Edit(recipientId, () => new DiscordUser(), user => user.Update(recipientData));
            }

            LastMessageId = data.GetString("last_message_id") ?? LastMessageId;
        }

        public override string ToString()
        {
            return $"DM Channel: {Recipient}";
        }
    }
}
