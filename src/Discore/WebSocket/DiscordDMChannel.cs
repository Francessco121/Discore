using System;
using System.Collections.Generic;
using Discore.Http.Net;

namespace Discore.WebSocket
{
    /// <summary>
    /// Direct message channels represent a one-to-one conversation between two users, outside of the scope of guilds.
    /// </summary>
    public sealed class DiscordDMChannel : DiscordChannel, ITextChannel
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
        /// Returns the message sent.
        /// </summary>
        public DiscordMessage SendMessage(string content, bool tts = false)
        {
            DiscordApiData data = channelsHttp.CreateMessage(Id, content, tts);

            DiscordMessage msg = new DiscordMessage(shard);
            msg.Update(data);

            return msg;
        }

        /// <summary>
        /// Sends a message with a file attachment to this direct message channel.
        /// Returns the message sent.
        /// </summary>
        public DiscordMessage SendMessage(string content, byte[] fileAttachment, bool tts = false)
        {
            DiscordApiData data = channelsHttp.UploadFile(Id, fileAttachment, content, tts);

            DiscordMessage msg = new DiscordMessage(shard);
            msg.Update(data);

            return msg;
        }

        public bool BulkDeleteMessages(IEnumerable<Snowflake> messageIds)
        {
            DiscordApiData data = channelsHttp.BulkDeleteMessages(Id, messageIds);
            return data.IsNull;
        }

        public bool TriggerTypingIndicator()
        {
            DiscordApiData data = channelsHttp.TriggerTypingIndicator(Id);
            return data.IsNull;
        }

        public IList<DiscordMessage> GetPinnedMessages()
        {
            DiscordApiData messagesArray = channelsHttp.GetPinnedMessages(Id);
            DiscordMessage[] messages = new DiscordMessage[messagesArray.Values.Count];
            
            for (int i = 0; i < messages.Length; i++)
            {
                DiscordMessage message = new DiscordMessage(shard);
                message.Update(messagesArray.Values[i]);

                messages[i] = message;
            }

            return messages;
        }

        public DiscordMessage GetMessage(Snowflake messageId)
        {
            DiscordApiData data = channelsHttp.GetMessage(Id, messageId);
            DiscordMessage message = new DiscordMessage(shard);
            message.Update(data);

            return message;
        }

        public IList<DiscordMessage> GetMessages(Snowflake? baseMessageId = null, int? limit = null, 
            DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before)
        {
            DiscordApiData messagesArray = channelsHttp.GetMessages(Id, baseMessageId, limit, getStrategy);
            DiscordMessage[] messages = new DiscordMessage[messagesArray.Values.Count];

            for (int i = 0; i < messages.Length; i++)
            {
                DiscordMessage message = new DiscordMessage(shard);
                message.Update(messagesArray.Values[i]);

                messages[i] = message;
            }

            return messages;
        }

        public bool Delete()
        {
            DiscordApiData data = channelsHttp.Delete(Id);
            return data.IsNull;
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
