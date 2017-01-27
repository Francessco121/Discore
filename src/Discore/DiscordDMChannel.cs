using Discore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore
{
    /// <summary>
    /// Direct message channels represent a one-to-one conversation between two users, outside of the scope of guilds.
    /// </summary>
    public sealed class DiscordDMChannel : DiscordChannel, ITextChannel
    {
        /// <summary>
        /// Gets the user on the other end of this channel.
        /// </summary>
        public DiscordUser Recipient { get { return cache != null ? cache.Users[recipientId] : recipient; } }

        IDiscordApplication app;
        DiscordHttpChannelEndpoint channelsHttp;
        Snowflake lastMessageId;

        DiscoreCache cache;
        DiscordUser recipient;
        Snowflake recipientId;

        internal DiscordDMChannel(DiscoreCache cache, IDiscordApplication app, DiscordApiData data)
            : this(app, data, true)
        {
            this.cache = cache;
        }

        internal DiscordDMChannel(IDiscordApplication app, DiscordApiData data)
            : this(app, data, false)
        { }

        private DiscordDMChannel(IDiscordApplication app, DiscordApiData data, bool isWebSocket) 
            : base(app, data, DiscordChannelType.DirectMessage)
        {
            this.app = app;
            channelsHttp = app.HttpApi.Channels;

            lastMessageId = data.GetSnowflake("last_message_id") ?? default(Snowflake);

            if (!isWebSocket)
            {
                DiscordApiData recipientData = data.Get("recipient");
                recipient = new DiscordUser(recipientData);
            }
            else
                recipientId = data.LocateSnowflake("recipient.id").Value;
        }

        /// <summary>
        /// Gets the id of the last message sent in this channel.
        /// </summary>
        public async Task<Snowflake> GetLastMessageId()
        {
            Snowflake lastId = lastMessageId;
            while (true)
            {
                IReadOnlyList<DiscordMessage> messages = await GetMessages(lastId, 100, DiscordMessageGetStrategy.After)
                    .ConfigureAwait(false);

                lastId = messages.Count == 0 ? default(Snowflake) : messages[0].Id;

                if (messages.Count < 100)
                    break;
            }

            lastMessageId = lastId;
            return lastId;
        }

        /// <summary>
        /// Sends a message to this channel.
        /// </summary>
        /// <param name="content">The message text content.</param>
        /// <param name="splitIfTooLong">Whether this message should be split into multiple messages if too long.</param>
        /// <param name="tts">Whether this should be played over text-to-speech.</param>
        /// <returns>Returns the created message (or first if split into multiple).</returns>
        public async Task<DiscordMessage> SendMessage(string content, bool splitIfTooLong = false, bool tts = false)
        {
            DiscordMessage firstOrOnlyMessage = null;

            if (splitIfTooLong && content.Length > DiscordMessage.MAX_CHARACTERS)
            {
                await SplitSendMessage(content,
                    async message =>
                    {
                        DiscordMessage msg = await channelsHttp.CreateMessage(Id, message, tts).ConfigureAwait(false);

                        if (firstOrOnlyMessage == null)
                            firstOrOnlyMessage = msg;
                    }).ConfigureAwait(false);
            }
            else
                firstOrOnlyMessage = await channelsHttp.CreateMessage(Id, content, tts).ConfigureAwait(false);

            return firstOrOnlyMessage;
        }

        /// <summary>
        /// Sends a message with a file attachment to this channel.
        /// </summary>
        /// <param name="fileAttachment">The file data to attach.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="content">The message text content.</param>
        /// <param name="splitIfTooLong">Whether this message should be split into multiple messages if too long.</param>
        /// <param name="tts">Whether this should be played over text-to-speech.</param>
        /// <returns>Returns the created message (or first if split into multiple).</returns>
        public async Task<DiscordMessage> SendMessage(byte[] fileAttachment, string fileName = null, string content = null,
            bool splitIfTooLong = false, bool tts = false)
        {
            DiscordMessage firstOrOnlyMessage = null;

            if (splitIfTooLong && content.Length > DiscordMessage.MAX_CHARACTERS)
            {
                await SplitSendMessage(content,
                    async message =>
                    {
                        if (firstOrOnlyMessage == null)
                        {
                            DiscordMessage msg = await channelsHttp.UploadFile(Id, fileAttachment, fileName, message, tts)
                                .ConfigureAwait(false);
                            firstOrOnlyMessage = msg;
                        }
                        else
                            await channelsHttp.CreateMessage(Id, message, tts).ConfigureAwait(false);
                    }).ConfigureAwait(false);
            }
            else
                firstOrOnlyMessage = await channelsHttp.UploadFile(Id, fileAttachment, fileName, content, tts).ConfigureAwait(false);

            return firstOrOnlyMessage;
        }

        async Task SplitSendMessage(string content, Func<string, Task> createMessageCallback)
        {
            int i = 0;
            while (i < content.Length)
            {
                int maxChars = Math.Min(DiscordMessage.MAX_CHARACTERS, content.Length - i);
                int lastNewLine = content.LastIndexOf('\n', i + maxChars - 1, maxChars - 1);

                string subMessage;
                if (lastNewLine > -1)
                    subMessage = content.Substring(i, lastNewLine - i);
                else
                    subMessage = content.Substring(i, maxChars);

                if (!string.IsNullOrWhiteSpace(subMessage))
                    await createMessageCallback(subMessage).ConfigureAwait(false);

                i += subMessage.Length;
            }
        }

        /// <summary>
        /// Deletes a list of messages in one API call.
        /// Much quicker than calling Delete() on each message instance.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public Task<bool> BulkDeleteMessages(IEnumerable<DiscordMessage> messages)
        {
            return channelsHttp.BulkDeleteMessages(Id, messages);
        }

        /// <summary>
        /// Deletes a list of messages in one API call.
        /// Much quicker than calling Delete() on each message instance.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public Task<bool> BulkDeleteMessages(IEnumerable<Snowflake> messageIds)
        {
            return channelsHttp.BulkDeleteMessages(Id, messageIds);
        }

        /// <summary>
        /// Causes the current authenticated user to appear as typing in this channel.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public Task<bool> TriggerTypingIndicator()
        {
            return channelsHttp.TriggerTypingIndicator(Id);
        }

        /// <summary>
        /// Gets a list of all pinned messages in this channel.
        /// </summary>
        public Task<IReadOnlyList<DiscordMessage>> GetPinnedMessages()
        {
            return channelsHttp.GetPinnedMessages(Id);
        }

        /// <summary>
        /// Gets a message in this channel.
        /// </summary>
        public Task<DiscordMessage> GetMessage(Snowflake messageId)
        {
            return channelsHttp.GetMessage(Id, messageId);
        }

        /// <summary>
        /// Gets a list of messages in this channel.
        /// </summary>
        /// <param name="baseMessageId">The message id the list will start at (is not included in the final list).</param>
        /// <param name="limit">Maximum number of messages to be returned.</param>
        /// <param name="getStrategy">The way messages will be located based on the <paramref name="baseMessageId"/>.</param>
        public Task<IReadOnlyList<DiscordMessage>> GetMessages(Snowflake baseMessageId, int? limit = null, 
            DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before)
        {
            return channelsHttp.GetMessages(Id, baseMessageId, limit, getStrategy);
        }

        public override string ToString()
        {
            return $"DM Channel: {Recipient}";
        }
    }
}
