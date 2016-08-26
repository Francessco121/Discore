using System.Threading.Tasks;

namespace Discore.Net
{
    /// <summary>
    /// A <see cref="IDiscordRestClient"/> service for interacting with <see cref="DiscordMessage"/>s.
    /// </summary>
    public interface IDiscordRestMessagesService
    {
        /// <summary>
        /// Posts a message to a text <see cref="DiscordGuildChannel"/> or a <see cref="DiscordDMChannel"/>.
        /// </summary>
        /// <param name="channel">The <see cref="DiscordChannel"/> to post the message to.</param>
        /// <param name="message">The message to post.</param>
        /// <returns>Returns the posted <see cref="DiscordMessage"/>.</returns>
        Task<DiscordMessage> Send(DiscordChannel channel, string message);

        /// <summary>
        /// Posts a message with a file attachment to a text <see cref="DiscordGuildChannel"/> 
        /// or a <see cref="DiscordDMChannel"/>.
        /// </summary>
        /// <param name="channel">The <see cref="DiscordChannel"/> to post the message to.</param>
        /// <param name="message">The message to post.</param>
        /// <param name="file">The contents of the file attachment.</param>
        /// <returns>Returns the posted <see cref="DiscordMessage"/>.</returns>
        Task<DiscordMessage> Send(DiscordChannel channel, string message, byte[] file);

        /// <summary>
        /// Gets a <see cref="DiscordMessage"/> from the specified <see cref="DiscordChannel"/>.
        /// </summary>
        /// <param name="channel">The <see cref="DiscordChannel"/> to get the message from.</param>
        /// <param name="messageId">The id of the <see cref="DiscordMessage"/> to retrieve.</param>
        /// <returns>Returns the <see cref="DiscordMessage"/> specified.</returns>
        Task<DiscordMessage> Get(DiscordChannel channel, string messageId);

        /// <summary>
        /// Gets a batch of <see cref="DiscordMessage"/>s from the specified <see cref="DiscordChannel"/>.
        /// </summary>
        /// <param name="channel">The <see cref="DiscordChannel"/> to get the messages from.</param>
        /// <param name="strategy">The way messages should be retrieved.</param>
        /// <param name="baseMessageId">The message id anchor for the specified <see cref="DiscordMessageGetStrategy"/>.</param>
        /// <param name="limit">The maximum number of <see cref="DiscordMessage"/>s to retrieve. Must be between 1 and 100.</param>
        /// <returns>Returns the retrieved <see cref="DiscordMessage"/>s.</returns>
        Task<DiscordMessage[]> Get(DiscordChannel channel, DiscordMessageGetStrategy strategy, string baseMessageId, int limit = 50);

        /// <summary>
        /// Edits a previously sent <see cref="DiscordMessage"/>.
        /// </summary>
        /// <param name="channel">The <see cref="DiscordChannel"/> the <see cref="DiscordMessage"/> is in.</param>
        /// <param name="messageId">The id of the <see cref="DiscordMessage"/>.</param>
        /// <param name="content">The new contents of the <see cref="DiscordMessage"/>.</param>
        /// <returns>Returns the editted <see cref="DiscordMessage"/>.</returns>
        Task<DiscordMessage> Edit(DiscordChannel channel, string messageId, string content);

        /// <summary>
        /// Deletes a <see cref="DiscordMessage"/>.
        /// </summary>
        /// <param name="channel">The <see cref="DiscordChannel"/> the <see cref="DiscordMessage"/> is in.</param>
        /// <param name="messageId">The id of the <see cref="DiscordMessage"/> to delete.</param>
        /// <returns>Returns whether or not the deletion was successful.</returns>
        Task<bool> Delete(DiscordChannel channel, string messageId);

        /// <summary>
        /// Deletes a batch of <see cref="DiscordMessage"/>s.
        /// </summary>
        /// <param name="channel">The <see cref="DiscordChannel"/> the <see cref="DiscordMessage"/>s are in.</param>
        /// <param name="messageIds">The ids of the <see cref="DiscordMessage"/>s to delete.</param>
        /// <returns>Returns whether or not the deletion was successful.</returns>
        Task<bool> Delete(DiscordChannel channel, string[] messageIds);
    }
}
