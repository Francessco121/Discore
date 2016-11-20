using Discore.Http.Net;
using System.Collections.Generic;

namespace Discore.WebSocket
{
    public sealed class DiscordGuildTextChannel : DiscordGuildChannel, ITextChannel
    {
        /// <summary>
        /// Gets the topic of this channel.
        /// </summary>
        public string Topic { get; private set; }

        /// <summary>
        /// Gets the id of the last message sent in this channel.
        /// </summary>
        public string LastMessageId { get; private set; }

        HttpChannelsEndpoint channelsHttp;

        internal DiscordGuildTextChannel(Shard shard, DiscordGuild guild)
            : base(shard, guild, DiscordGuildChannelType.Text)
        {
            channelsHttp = shard.Application.InternalHttpApi.Channels;
        }

        /// <summary>
        /// Sends a message to this guild channel.
        /// Returns the message sent.
        /// </summary>
        public DiscordMessage SendMessage(string content, bool tts = false)
        {
            DiscordApiData data = channelsHttp.CreateMessage(Id, content, tts);

            DiscordMessage msg = new DiscordMessage(Shard);
            msg.Update(data);

            return msg;
        }

        /// <summary>
        /// Sends a message with a file attachment to this guild channel.
        /// Returns the message sent.
        /// </summary>
        public DiscordMessage SendMessage(string content, byte[] fileAttachment, bool tts = false)
        {
            DiscordApiData data = channelsHttp.UploadFile(Id, fileAttachment, content, tts);

            DiscordMessage msg = new DiscordMessage(Shard);
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
                DiscordMessage message = new DiscordMessage(Shard);
                message.Update(messagesArray.Values[i]);

                messages[i] = message;
            }

            return messages;
        }

        public DiscordMessage GetMessage(Snowflake messageId)
        {
            DiscordApiData data = channelsHttp.GetMessage(Id, messageId);
            DiscordMessage message = new DiscordMessage(Shard);
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
                DiscordMessage message = new DiscordMessage(Shard);
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

            Topic = data.GetString("topic") ?? Topic;
            LastMessageId = data.GetString("last_message_id") ?? LastMessageId;
        }
    }
}
