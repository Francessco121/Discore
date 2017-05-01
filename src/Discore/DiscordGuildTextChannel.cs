using Discore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discore
{
    public sealed class DiscordGuildTextChannel : DiscordGuildChannel, ITextChannel
    {
        /// <summary>
        /// Gets the topic of this channel.
        /// </summary>
        public string Topic { get; }

        IDiscordApplication app;
        DiscordHttpChannelEndpoint channelsHttp;
        DiscordHttpWebhookEndpoint webhookHttp;
        Snowflake lastMessageId;

        internal DiscordGuildTextChannel(IDiscordApplication app, DiscordApiData data, Snowflake? guildId = null)
            : base(app, data, DiscordGuildChannelType.Text, guildId)
        {
            this.app = app;
            channelsHttp = app.HttpApi.Channels;
            webhookHttp = app.HttpApi.Webhooks;

            Topic = data.GetString("topic");
            lastMessageId = data.GetSnowflake("last_message_id") ?? default(Snowflake);
        }

        /// <summary>
        /// Gets a list of all webhooks for this channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordWebhook>> GetWebhooks()
        {
            return webhookHttp.GetChannelWebhooks(Id);
        }

        /// <summary>
        /// Gets the id of the last message sent in this channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException">Thrown if failed to retrieve channel messages.</exception>
        public async Task<Snowflake> GetLastMessageId()
        {
            Snowflake lastId = lastMessageId;
            while (true)
            {
                IReadOnlyList<DiscordMessage> messages = await GetMessages(lastId, 100, DiscordMessageGetStrategy.After)
                    .ConfigureAwait(false);

                lastId = messages.Count == 0 ? lastId : messages[0].Id;

                if (messages.Count < 100)
                    break;
            }

            lastMessageId = lastId;
            return lastId;
        }

        #region Deprecated Modify
        /// <summary>
        /// Modifies this text channel.
        /// Any parameters not specified will be unchanged.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        [Obsolete("Please use the Modify overload with a builder object instead.")]
        public Task<DiscordGuildTextChannel> Modify(string name = null, int? position = null, string topic = null)
        {
            return channelsHttp.Modify<DiscordGuildTextChannel>(Id, name, position, topic);
        }
        #endregion

        /// <summary>
        /// Modifies this text channel's settings.
        /// </summary>
        /// <param name="parameters">A set of parameters to modify the channel with</param>
        /// <returns>Returns the updated text channel.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildTextChannel> Modify(GuildTextChannelParameters parameters)
        {
            return channelsHttp.ModifyTextChannel(Id, parameters);
        }

        #region Deprecated SendMessage
        /// <summary>
        /// Sends a message to this channel.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// </summary>
        /// <param name="content">The message text content.</param>
        /// <param name="splitIfTooLong">Whether this message should be split into multiple messages if too long.</param>
        /// <param name="tts">Whether this should be played over text-to-speech.</param>
        /// <returns>Returns the created message (or first if split into multiple).</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        [Obsolete("Please use CreateMessage instead.")]
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
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// </summary>
        /// <param name="fileAttachment">The file data to attach.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="content">The message text content.</param>
        /// <param name="splitIfTooLong">Whether this message should be split into multiple messages if too long.</param>
        /// <param name="tts">Whether this should be played over text-to-speech.</param>
        /// <returns>Returns the created message (or first if split into multiple).</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        [Obsolete("Please use UploadFile instead.")]
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
                            DiscordMessage msg = await channelsHttp.UploadFile(Id, fileAttachment, fileName, content, tts).ConfigureAwait(false);
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
        #endregion

        /// <summary>
        /// Creates a message in this channel.
        /// </summary>
        /// <param name="content">The message text content.</param>
        /// <returns>Returns the created message.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> CreateMessage(string content)
        {
            return channelsHttp.CreateMessage(Id, content);
        }

        /// <summary>
        /// Creates a message in this channel.
        /// </summary>
        /// <param name="details">The details of the message to create.</param>
        /// <returns>Returns the created message.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> CreateMessage(DiscordMessageDetails details)
        {
            return channelsHttp.CreateMessage(Id, details);
        }

        /// <summary>
        /// Uploads a file with an optional message to this channel.
        /// </summary>
        /// <param name="fileData">A stream of the file contents.</param>
        /// <param name="fileName">The name of the file to use when uploading.</param>
        /// <param name="details">Optional extra details of the message to create.</param>
        /// <returns>Returns the created message.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> UploadFile(Stream fileData, string fileName, DiscordMessageDetails details = null)
        {
            return channelsHttp.UploadFile(Id, fileData, fileName, details);
        }
        /// <summary>
        /// Uploads a file with an optional message to this channel.
        /// </summary>
        /// <param name="fileData">The file contents.</param>
        /// <param name="fileName">The name of the file to use when uploading.</param>
        /// <param name="details">Optional extra details of the message to create.</param>
        /// <returns>Returns the created message.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> UploadFile(ArraySegment<byte> fileData, string fileName, DiscordMessageDetails details = null)
        {
            return channelsHttp.UploadFile(Id, fileData, fileName, details);
        }

        /// <summary>
        /// Deletes a list of messages in one API call.
        /// Much quicker than calling Delete() on each message instance.
        /// </summary>
        /// <param name="filterTooOldMessages">Whether to ignore deleting messages that are older than 2 weeks
        /// (messages that are too old cause an API error).</param>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> BulkDeleteMessages(IEnumerable<DiscordMessage> messages, bool filterTooOldMessages = true)
        {
            return channelsHttp.BulkDeleteMessages(Id, messages, filterTooOldMessages);
        }

        /// <summary>
        /// Deletes a list of messages in one API call.
        /// Much quicker than calling Delete() on each message instance.
        /// </summary>
        /// <param name="filterTooOldMessages">Whether to ignore deleting messages that are older than 2 weeks
        /// (messages that are too old cause an API error).</param>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> BulkDeleteMessages(IEnumerable<Snowflake> messageIds, bool filterTooOldMessages = true)
        {
            return channelsHttp.BulkDeleteMessages(Id, messageIds, filterTooOldMessages);
        }

        /// <summary>
        /// Causes the current authenticated user to appear as typing in this channel.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> TriggerTypingIndicator()
        {
            return channelsHttp.TriggerTypingIndicator(Id);
        }

        /// <summary>
        /// Gets a list of all pinned messages in this channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordMessage>> GetPinnedMessages()
        {
            return channelsHttp.GetPinnedMessages(Id);
        }

        /// <summary>
        /// Gets a message in this channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
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
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordMessage>> GetMessages(Snowflake baseMessageId, int? limit = null,
            DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before)
        {
            return channelsHttp.GetMessages(Id, baseMessageId, limit, getStrategy);
        }
    }
}
