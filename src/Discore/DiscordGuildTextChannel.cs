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

        /// <summary>
        /// Gets whether this text channel is NSFW (not-safe-for-work).
        /// </summary>
        public bool Nsfw { get; }

        DiscordHttpClient http;
        Snowflake lastMessageId;

        internal DiscordGuildTextChannel(DiscordHttpClient http, DiscordApiData data, Snowflake? guildId = null)
            : base(http, data, DiscordChannelType.GuildText, guildId)
        {
            this.http = http;

            Topic = data.GetString("topic");
            Nsfw = data.GetBoolean("nsfw") ?? false;
            lastMessageId = data.GetSnowflake("last_message_id") ?? default(Snowflake);
        }

        /// <summary>
        /// Gets a list of all webhooks for this channel.
        /// <para>Requires <see cref="DiscordPermission.ManageWebhooks"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordWebhook>> GetWebhooks()
        {
            return http.GetChannelWebhooks(Id);
        }

        /// <summary>
        /// Gets the ID of the last message sent in this channel.
        /// <para>Requires <see cref="DiscordPermission.ReadMessages"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException">Thrown if failed to retrieve channel messages.</exception>
        public async Task<Snowflake> GetLastMessageId()
        {
            Snowflake lastId = lastMessageId;
            while (true)
            {
                IReadOnlyList<DiscordMessage> messages = await GetMessages(lastId, 100, MessageGetStrategy.After)
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
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <param name="options">A set of options to modify the channel with</param>
        /// <returns>Returns the updated text channel.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildTextChannel> Modify(GuildTextChannelOptions options)
        {
            return http.ModifyTextChannel(Id, options);
        }

        /// <summary>
        /// Creates a message in this channel.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// <para>Requires <see cref="DiscordPermission.SendMessages"/>.</para>
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
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// <para>Requires <see cref="DiscordPermission.SendMessages"/>.</para>
        /// <para>Requires <see cref="DiscordPermission.SendTtsMessages"/> if TTS is enabled on the message.</para>
        /// </summary>
        /// <param name="details">The details of the message to create.</param>
        /// <returns>Returns the created message.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> CreateMessage(CreateMessageOptions details)
        {
            return http.CreateMessage(Id, details);
        }

        /// <summary>
        /// Posts a message with a file attachment.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// <para>Requires <see cref="DiscordPermission.SendMessages"/>.</para>
        /// <para>Requires <see cref="DiscordPermission.SendTtsMessages"/> if TTS is enabled on the message.</para>
        /// </summary>
        /// <param name="fileData">A stream of the file contents.</param>
        /// <param name="fileName">The name of the file to use when uploading.</param>
        /// <param name="details">Optional extra details of the message to create.</param>
        /// <returns>Returns the created message.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> CreateMessage(Stream fileData, string fileName, CreateMessageOptions details = null)
        {
            return http.CreateMessage(Id, fileData, fileName, details);
        }
        /// <summary>
        /// Posts a message with a file attachment.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// <para>Requires <see cref="DiscordPermission.SendMessages"/>.</para>
        /// <para>Requires <see cref="DiscordPermission.SendTtsMessages"/> if TTS is enabled on the message.</para>
        /// </summary>
        /// <param name="fileData">The file contents.</param>
        /// <param name="fileName">The name of the file to use when uploading.</param>
        /// <param name="details">Optional extra details of the message to create.</param>
        /// <returns>Returns the created message.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> CreateMessage(ArraySegment<byte> fileData, string fileName, CreateMessageOptions details = null)
        {
            return http.CreateMessage(Id, fileData, fileName, details);
        }

        /// <summary>
        /// Deletes a list of messages in one API call.
        /// Much quicker than calling Delete() on each message instance.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
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
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
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
        /// Causes the current bot's user to appear as typing in this channel.
        /// <para>Note: it is recommended that bots do not generally use this route.
        /// This should only be used if the bot is responding to a command that is expected
        /// to take a few seconds or longer.</para>
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
        /// <para>Requires <see cref="DiscordPermission.ReadMessageHistory"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> GetMessage(Snowflake messageId)
        {
            return http.GetChannelMessage(Id, messageId);
        }

        /// <summary>
        /// Gets a list of messages in this channel.
        /// <para>Requires <see cref="DiscordPermission.ReadMessages"/>.</para>
        /// </summary>
        /// <param name="baseMessageId">The message ID the list will start at (is not included in the final list).</param>
        /// <param name="limit">Maximum number of messages to be returned.</param>
        /// <param name="getStrategy">The way messages will be located based on the <paramref name="baseMessageId"/>.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordMessage>> GetMessages(Snowflake baseMessageId, int? limit = null,
            MessageGetStrategy getStrategy = MessageGetStrategy.Before)
        {
            return http.GetChannelMessages(Id, baseMessageId, limit, getStrategy);
        }
    }
}
