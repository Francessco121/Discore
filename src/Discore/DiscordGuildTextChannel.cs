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

        DiscordHttpClient http;
        Snowflake lastMessageId;

        internal DiscordGuildTextChannel(DiscordHttpClient http, DiscordApiData data, Snowflake? guildId = null)
            : base(http, data, DiscordGuildChannelType.Text, guildId)
        {
            this.http = http;

            Topic = data.GetString("topic");
            lastMessageId = data.GetSnowflake("last_message_id") ?? default(Snowflake);
        }

        /// <summary>
        /// Gets a list of all webhooks for this channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordWebhook>> GetWebhooks()
        {
            return http.GetChannelWebhooks(Id);
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

        /// <summary>
        /// Modifies this text channel's settings.
        /// </summary>
        /// <param name="parameters">A set of parameters to modify the channel with</param>
        /// <returns>Returns the updated text channel.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildTextChannel> Modify(GuildTextChannelParameters parameters)
        {
            return http.ModifyTextChannel(Id, parameters);
        }

        /// <summary>
        /// Creates a message in this channel.
        /// </summary>
        /// <param name="content">The message text content.</param>
        /// <returns>Returns the created message.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> CreateMessage(string content)
        {
            return http.CreateMessage(Id, content);
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
            return http.CreateMessage(Id, details);
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
            return http.CreateMessage(Id, fileData, fileName, details);
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
            return http.CreateMessage(Id, fileData, fileName, details);
        }

        /// <summary>
        /// Deletes a list of messages in one API call.
        /// Much quicker than calling Delete() on each message instance.
        /// </summary>
        /// <param name="filterTooOldMessages">Whether to ignore deleting messages that are older than 2 weeks
        /// (messages that are too old cause an API error).</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task BulkDeleteMessages(IEnumerable<DiscordMessage> messages, bool filterTooOldMessages = true)
        {
            return http.BulkDeleteMessages(Id, messages, filterTooOldMessages);
        }

        /// <summary>
        /// Deletes a list of messages in one API call.
        /// Much quicker than calling Delete() on each message instance.
        /// </summary>
        /// <param name="filterTooOldMessages">Whether to ignore deleting messages that are older than 2 weeks
        /// (messages that are too old cause an API error).</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task BulkDeleteMessages(IEnumerable<Snowflake> messageIds, bool filterTooOldMessages = true)
        {
            return http.BulkDeleteMessages(Id, messageIds, filterTooOldMessages);
        }

        /// <summary>
        /// Causes the current authenticated user to appear as typing in this channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task TriggerTypingIndicator()
        {
            return http.TriggerTypingIndicator(Id);
        }

        /// <summary>
        /// Gets a list of all pinned messages in this channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordMessage>> GetPinnedMessages()
        {
            return http.GetPinnedMessages(Id);
        }

        /// <summary>
        /// Gets a message in this channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> GetMessage(Snowflake messageId)
        {
            return http.GetChannelMessage(Id, messageId);
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
            return http.GetChannelMessages(Id, baseMessageId, limit, getStrategy);
        }
    }
}
