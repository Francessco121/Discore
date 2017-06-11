using Discore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Discore
{
    public interface ITextChannel
    {
        /// <summary>
        /// Gets the ID of this text channel.
        /// </summary>
        Snowflake Id { get; }
        /// <summary>
        /// Gets the type of this channel.
        /// </summary>
        DiscordChannelType ChannelType { get; }

        /// <summary>
        /// Creates a message in this channel.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// <para>Requires <see cref="DiscordPermission.SendMessages"/>.</para>
        /// </summary>
        /// <param name="content">The message text content.</param>
        /// <returns>Returns the created message.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        Task<DiscordMessage> CreateMessage(string content);
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
        Task<DiscordMessage> CreateMessage(DiscordMessageDetails details);
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
        Task<DiscordMessage> CreateMessage(Stream fileData, string fileName, DiscordMessageDetails details = null);
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
        Task<DiscordMessage> CreateMessage(ArraySegment<byte> fileData, string fileName, DiscordMessageDetails details = null);

        /// <summary>
        /// Deletes a list of messages in one API call.
        /// Much quicker than calling Delete() on each message instance.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// <para>Note: if this is a DM channel, this can only delete messages sent by the current bot.</para>
        /// </summary>
        /// <param name="filterTooOldMessages">Whether to ignore deleting messages that are older than 2 weeks 
        /// (messages that are too old cause an API error).</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        Task BulkDeleteMessages(IEnumerable<DiscordMessage> messages, bool filterTooOldMessages = true);
        /// <summary>
        /// Deletes a list of messages in one API call.
        /// Much quicker than calling Delete() on each message instance.
        /// <para>Requires <see cref="DiscordPermission.ManageMessages"/>.</para>
        /// <para>Note: if this is a DM channel, this can only delete messages sent by the current bot.</para>
        /// </summary>
        /// <param name="filterTooOldMessages">Whether to ignore deleting messages that are older than 2 weeks 
        /// (messages that are too old cause an API error).</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        Task BulkDeleteMessages(IEnumerable<Snowflake> messageIds, bool filterTooOldMessages = true);

        /// <summary>
        /// Causes the current authenticated user to appear as typing in this channel.
        /// <para>Note: it is recommended that bots do not generally use this route.
        /// This should only be used if the bot is responding to a command that is expected
        /// to take a few seconds or longer.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        Task TriggerTypingIndicator();

        /// <summary>
        /// Gets a message in this channel.
        /// <para>Requires <see cref="DiscordPermission.ReadMessageHistory"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        Task<DiscordMessage> GetMessage(Snowflake messageId);
        /// <summary>
        /// Gets a list of messages in this channel.
        /// <para>Requires <see cref="DiscordPermission.ReadMessages"/>.</para>
        /// </summary>
        /// <param name="baseMessageId">The message id the list will start at (is not included in the final list).</param>
        /// <param name="limit">Maximum number of messages to be returned.</param>
        /// <param name="getStrategy">The way messages will be located based on the <paramref name="baseMessageId"/>.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        Task<IReadOnlyList<DiscordMessage>> GetMessages(Snowflake baseMessageId, int? limit = null,
            MessageGetStrategy getStrategy = MessageGetStrategy.Before);
        /// <summary>
        /// Gets a list of all pinned messages in this channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        Task<IReadOnlyList<DiscordMessage>> GetPinnedMessages();

        /// <summary>
        /// Gets the id of the last message sent in this channel.
        /// <para>Requires <see cref="DiscordPermission.ReadMessages"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        Task<Snowflake> GetLastMessageId();
    }
}
