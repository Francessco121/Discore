﻿using System.Collections.Generic;
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
        /// Sends a message to this channel.
        /// </summary>
        /// <param name="content">The message text content.</param>
        /// <param name="splitIfTooLong">Whether this message should be split into multiple messages if too long.</param>
        /// <param name="tts">Whether this should be played over text-to-speech.</param>
        /// <returns>Returns the created message (or first if split into multiple).</returns>
        Task<DiscordMessage> SendMessage(string content, bool splitIfTooLong = false, bool tts = false);
        /// <summary>
        /// Sends a message with a file attachment to this channel.
        /// </summary>
        /// <param name="fileAttachment">The file data to attach.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="content">The message text content.</param>
        /// <param name="splitIfTooLong">Whether this message should be split into multiple messages if too long.</param>
        /// <param name="tts">Whether this should be played over text-to-speech.</param>
        /// <returns>Returns the created message (or first if split into multiple).</returns>
        Task<DiscordMessage> SendMessage(byte[] fileAttachment, string fileName = null, string content = null, bool splitIfTooLong = false, bool tts = false);
        /// <summary>
        /// Deletes a list of messages in one API call.
        /// Much quicker than calling Delete() on each message instance.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        Task<bool> BulkDeleteMessages(IEnumerable<DiscordMessage> messages);
        /// <summary>
        /// Deletes a list of messages in one API call.
        /// Much quicker than calling Delete() on each message instance.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        Task<bool> BulkDeleteMessages(IEnumerable<Snowflake> messageIds);

        /// <summary>
        /// Causes the current authenticated user to appear as typing in this channel.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        Task<bool> TriggerTypingIndicator();

        /// <summary>
        /// Gets a message in this channel.
        /// </summary>
        Task<DiscordMessage> GetMessage(Snowflake messageId);
        /// <summary>
        /// Gets a list of messages in this channel.
        /// </summary>
        /// <param name="baseMessageId">The message id the list will start at (is not included in the final list).</param>
        /// <param name="limit">Maximum number of messages to be returned.</param>
        /// <param name="getStrategy">The way messages will be located based on the <paramref name="baseMessageId"/>.</param>
        Task<IReadOnlyList<DiscordMessage>> GetMessages(Snowflake baseMessageId, int? limit = null,
            DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before);
        /// <summary>
        /// Gets a list of all pinned messages in this channel.
        /// </summary>
        Task<IReadOnlyList<DiscordMessage>> GetPinnedMessages();

        /// <summary>
        /// Gets the id of the last message sent in this channel.
        /// </summary>
        Task<Snowflake> GetLastMessageId();
    }
}
