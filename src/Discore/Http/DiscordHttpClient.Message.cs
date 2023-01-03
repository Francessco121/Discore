using Discore.Http.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets messages from a text channel.
        /// <para>Requires <see cref="DiscordPermission.ReadMessages"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordMessage>> GetChannelMessages(Snowflake channelId,
            Snowflake? baseMessageId = null, int? limit = null,
            MessageGetStrategy getStrategy = MessageGetStrategy.Before)
        {
            var builder = new UrlParametersBuilder();
            if (baseMessageId.HasValue)
                builder.Add(getStrategy.ToString().ToLower(), baseMessageId.Value.ToString());
            if (limit.HasValue)
                builder.Add("limit", limit.Value.ToString());

            using JsonDocument? data = await rest.Get($"channels/{channelId}/messages{builder.ToQueryString()}",
                $"channels/{channelId}/messages").ConfigureAwait(false);

            JsonElement values = data!.RootElement;

            var messages = new DiscordMessage[values.GetArrayLength()];

            for (int i = 0; i < messages.Length; i++)
                messages[i] = new DiscordMessage(values[i]);

            return messages;
        }

        /// <summary>
        /// Gets messages from a text channel.
        /// <para>Requires <see cref="DiscordPermission.ReadMessages"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordMessage>> GetChannelMessages(ITextChannel channel,
            Snowflake? baseMessageId = null, int? limit = null,
            MessageGetStrategy getStrategy = MessageGetStrategy.Before)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            return GetChannelMessages(channel.Id,
                baseMessageId: baseMessageId,
                limit: limit,
                getStrategy: getStrategy);
        }

        /// <summary>
        /// Gets a single message by ID from a channel.
        /// <para>Requires <see cref="DiscordPermission.ReadMessageHistory"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordMessage> GetChannelMessage(Snowflake channelId, Snowflake messageId)
        {
            using JsonDocument? data = await rest.Get($"channels/{channelId}/messages/{messageId}",
                $"channels/{channelId}/messages/message").ConfigureAwait(false);

            return new DiscordMessage(data!.RootElement);
        }

        /// <summary>
        /// Gets a single message by ID from a channel.
        /// <para>Requires <see cref="DiscordPermission.ReadMessageHistory"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> GetChannelMessage(ITextChannel channel, Snowflake messageId)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            return GetChannelMessage(channel.Id, messageId);
        }

        /// <summary>
        /// Posts a message to a text channel.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// <para>Requires <see cref="DiscordPermission.SendMessages"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> CreateMessage(Snowflake channelId, string content)
        {
            return CreateMessageInternal(channelId, new CreateMessageOptions(content));
        }

        /// <summary>
        /// Posts a message to a text channel.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// <para>Requires <see cref="DiscordPermission.SendMessages"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> CreateMessage(ITextChannel channel, string content)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            return CreateMessageInternal(channel.Id, new CreateMessageOptions(content));
        }

        /// <summary>
        /// Posts a message to a text channel.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// <para>Requires <see cref="DiscordPermission.SendMessages"/>.</para>
        /// <para>Requires <see cref="DiscordPermission.SendTtsMessages"/> if TTS is enabled on the message.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> CreateMessage(Snowflake channelId, CreateMessageOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            return CreateMessageInternal(channelId, options);
        }

        /// <summary>
        /// Posts a message to a text channel.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// <para>Requires <see cref="DiscordPermission.SendMessages"/>.</para>
        /// <para>Requires <see cref="DiscordPermission.SendTtsMessages"/> if TTS is enabled on the message.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="channel"/> or <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> CreateMessage(ITextChannel channel, CreateMessageOptions options)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (options == null) throw new ArgumentNullException(nameof(options));

            return CreateMessageInternal(channel.Id, options);
        }

        async Task<DiscordMessage> CreateMessageInternal(Snowflake channelId, CreateMessageOptions options)
        {
            // Determine if we can make a normal JSON request or if we need multipart form data for file uploads
            if (options.Attachments == null || !options.Attachments.Any((a) => a.Content != null))
            {
                string requestData = BuildJsonContent(options.Build);

                using JsonDocument? returnData = await rest.Post($"channels/{channelId}/messages", jsonContent: requestData,
                    $"channels/{channelId}/messages").ConfigureAwait(false);

                return new DiscordMessage(returnData!.RootElement);
            }
            else
            {
                using JsonDocument? returnData = await rest.Send(() =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post,
                        $"{RestClient.BASE_URL}/channels/{channelId}/messages");

                    var data = new MultipartFormDataContent();

                    foreach (AttachmentOptions attachment in options.Attachments)
                    {
                        if (attachment.Content != null)
                            data.Add(attachment.Content, $"files[{attachment.Id}]", attachment.FileName);
                    }

                    string payloadJson = BuildJsonContent(options.Build);
                    data.Add(new StringContent(payloadJson, Encoding.UTF8, "application/json"), "payload_json");

                    request.Content = data;

                    return request;
                }, $"channels/{channelId}/messages").ConfigureAwait(false);

                return new DiscordMessage(returnData!.RootElement);
            }
        }

        /// <summary>
        /// Edits an existing message in a text channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> EditMessage(Snowflake channelId, Snowflake messageId, string content)
        {
            return EditMessageInternal(channelId, messageId, new EditMessageOptions(content));
        }

        /// <summary>
        /// Edits an existing message in a text channel.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> EditMessage(DiscordMessage message, string content)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            return EditMessageInternal(message.ChannelId, message.Id, new EditMessageOptions(content));
        }

        /// <summary>
        /// Edits an existing message in a text channel.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> EditMessage(Snowflake channelId, Snowflake messageId, EditMessageOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            return EditMessageInternal(channelId, messageId, options);
        }

        /// <summary>
        /// Edits an existing message in a text channel.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> EditMessage(DiscordMessage message, EditMessageOptions options)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (options == null) throw new ArgumentNullException(nameof(options));

            return EditMessageInternal(message.ChannelId, message.Id, options);
        }

        async Task<DiscordMessage> EditMessageInternal(Snowflake channelId, Snowflake messageId, EditMessageOptions options)
        {
            // Determine if we can make a normal JSON request or if we need multipart form data for file uploads
            if (options.Attachments == null || !options.Attachments.Any((a) => a.Content != null))
            {
                string requestData = BuildJsonContent(options.Build);

                using JsonDocument? returnData = await rest.Patch($"channels/{channelId}/messages/{messageId}", jsonContent: requestData,
                    $"channels/{channelId}/messages/message").ConfigureAwait(false);

                return new DiscordMessage(returnData!.RootElement);
            }
            else
            {
                using JsonDocument? returnData = await rest.Send(() =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Patch,
                        $"{RestClient.BASE_URL}/channels/{channelId}/messages/{messageId}");

                    var data = new MultipartFormDataContent();

                    foreach (AttachmentOptions attachment in options.Attachments)
                    {
                        if (attachment.Content != null)
                            data.Add(attachment.Content, $"files[{attachment.Id}]", attachment.FileName);
                    }

                    string payloadJson = BuildJsonContent(options.Build);
                    data.Add(new StringContent(payloadJson, Encoding.UTF8, "application/json"), "payload_json");

                    request.Content = data;

                    return request;
                }, $"channels/{channelId}/messages/message").ConfigureAwait(false);

                return new DiscordMessage(returnData!.RootElement);
            }
        }

        /// <summary>
        /// Deletes a message from a text channel.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteMessage(Snowflake channelId, Snowflake messageId)
        {
            await rest.Delete($"channels/{channelId}/messages/{messageId}",
                $"channels/{channelId}/messages/message/delete").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a message from a text channel.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task DeleteMessage(DiscordMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            return DeleteMessage(message.ChannelId, message.Id);
        }

        /// <summary>
        /// Deletes a group of messages all at once from a text channel.
        /// This is much faster than calling DeleteMessage for each message.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// </summary>
        /// <param name="filterTooOldMessages">Whether to ignore deleting messages that are older than 2 weeks (this causes an API error).</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task BulkDeleteMessages(Snowflake channelId, IEnumerable<DiscordMessage> messages,
            bool filterTooOldMessages = true)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            var msgIds = new List<Snowflake>();
            foreach (DiscordMessage msg in messages)
                msgIds.Add(msg.Id);

            return BulkDeleteMessages(channelId, msgIds, filterTooOldMessages);
        }

        /// <summary>
        /// Deletes a group of messages all at once from a text channel.
        /// This is much faster than calling DeleteMessage for each message.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// </summary>
        /// <param name="filterTooOldMessages">Whether to ignore deleting messages that are older than 2 weeks (this causes an API error).</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task BulkDeleteMessages(ITextChannel channel, IEnumerable<DiscordMessage> messages,
            bool filterTooOldMessages = true)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            return BulkDeleteMessages(channel.Id, messages, filterTooOldMessages);
        }

        /// <summary>
        /// Deletes a group of messages all at once from a text channel.
        /// This is much faster than calling DeleteMessage for each message.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// </summary>
        /// <param name="filterTooOldMessages">Whether to ignore deleting messages that are older than 2 weeks (this causes an API error).</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task BulkDeleteMessages(Snowflake channelId, IEnumerable<Snowflake> messageIds,
            bool filterTooOldMessages = true)
        {
            if (messageIds == null)
                throw new ArgumentNullException(nameof(messageIds));

            string requestData = BuildJsonContent(writer =>
            {
                writer.WriteStartObject();

                writer.WriteStartArray("messages");

                ulong minimumAllowedSnowflake = 0;
                if (filterTooOldMessages)
                {
                    // See https://github.com/hammerandchisel/discord-api-docs/issues/208

                    ulong secondsSinceUnixEpoch = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds();
                    minimumAllowedSnowflake = (secondsSinceUnixEpoch - 14 * 24 * 60 * 60) * 1000 - 1420070400000L << 22;
                }

                foreach (Snowflake messageId in messageIds)
                {
                    if (!filterTooOldMessages && messageId.Id < minimumAllowedSnowflake)
                        continue;

                    writer.WriteSnowflakeValue(messageId);
                }

                writer.WriteEndArray();

                writer.WriteEndObject();
            });

            await rest.Post($"channels/{channelId}/messages/bulk-delete", requestData,
                $"channels/{channelId}/messages/message/delete/bulk").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a group of messages all at once from a text channel.
        /// This is much faster than calling DeleteMessage for each message.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// </summary>
        /// <param name="filterTooOldMessages">Whether to ignore deleting messages that are older than 2 weeks (this causes an API error).</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task BulkDeleteMessages(ITextChannel channel, IEnumerable<Snowflake> messageIds,
            bool filterTooOldMessages = true)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            return BulkDeleteMessages(channel.Id, messageIds, filterTooOldMessages);
        }

        /// <summary>
        /// Gets a list of all pinned messages in a text channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessages(Snowflake channelId)
        {
            using JsonDocument? data = await rest.Get($"channels/{channelId}/pins",
                $"channels/{channelId}/pins").ConfigureAwait(false);

            JsonElement values = data!.RootElement;

            var messages = new DiscordMessage[values.GetArrayLength()];

            for (int i = 0; i < messages.Length; i++)
                messages[i] = new DiscordMessage(values[i]);

            return messages;
        }

        /// <summary>
        /// Gets a list of all pinned messages in a text channel.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordMessage>> GetPinnedMessages(ITextChannel channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));

            return GetPinnedMessages(channel.Id);
        }

        /// <summary>
        /// Pins a message in a text channel.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task AddPinnedChannelMessage(Snowflake channelId, Snowflake messageId)
        {
            await rest.Put($"channels/{channelId}/pins/{messageId}",
                $"channels/{channelId}/pins/message").ConfigureAwait(false);
        }

        /// <summary>
        /// Pins a message in a text channel.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task AddPinnedChannelMessage(DiscordMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            return AddPinnedChannelMessage(message.ChannelId, message.Id);
        }

        /// <summary>
        /// Unpins a message from a text channel.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeletePinnedChannelMessage(Snowflake channelId, Snowflake messageId)
        {
            await rest.Delete($"channels/{channelId}/pins/{messageId}",
                $"channels/{channelId}/pins/message").ConfigureAwait(false);
        }

        /// <summary>
        /// Unpins a message from a text channel.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task DeletePinnedChannelMessage(DiscordMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            return DeletePinnedChannelMessage(message.ChannelId, message.Id);
        }
    }
}
