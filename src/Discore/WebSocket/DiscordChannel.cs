using Discore.Http.Net;
using System;

namespace Discore.WebSocket
{
    /// <summary>
    /// A <see cref="DiscordDMChannel"/> or a <see cref="DiscordGuildChannel"/>.
    /// </summary>
    public abstract class DiscordChannel : DiscordIdObject
    {
        /// <summary>
        /// Gets the type of this channel.
        /// </summary>
        public DiscordChannelType ChannelType { get; }

        protected Shard Shard { get; }

        DiscordRestApi rest;

        internal DiscordChannel(Shard shard, DiscordChannelType type)
        {
            Shard = shard;
            ChannelType = type;

            rest = shard.Application.Rest;
        }

        ///// <summary>
        ///// Closes the channel.
        ///// </summary>
        //public void Close()
        //{
        //    Rest.Channels.Close(this);
        //}

        ///// <summary>
        ///// Sends a message to this channel.
        ///// </summary>
        ///// <param name="message">The message to send.</param>
        ///// <param name="splitIfTooLong">Whether to split this call into multiple messages if above 2000 characters.</param>
        ///// <remarks>
        ///// If <code>splitIfTooLong</code> is set to true, the message returned will be the first chunk sent.
        ///// The message will be split by using newlines if possible to ensure the middle of a sentence isn't broken.
        ///// </remarks>
        //public DiscordMessage SendMessage(string message, bool splitIfTooLong = false)
        //{
        //    if (splitIfTooLong && message.Length > 2000)
        //    {
        //        DiscordMessage firstMsg = null;

        //        int i = 0;
        //        while (i < message.Length)
        //        {
        //            int maxChars = Math.Min(2000, message.Length - i);
        //            int lastNewLine = message.LastIndexOf('\n', i + maxChars - 1, maxChars - 1);

        //            string subMessage;
        //            if (lastNewLine > -1)
        //                subMessage = message.Substring(i, lastNewLine - i);
        //            else
        //                subMessage = message.Substring(i, maxChars);

        //            if (!string.IsNullOrWhiteSpace(subMessage))
        //            {
        //                DiscordMessage msg = Rest.Messages.Send(this, subMessage);

        //                if (firstMsg == null)
        //                    firstMsg = msg;
        //            }

        //            i += subMessage.Length;
        //        }

        //        return firstMsg;
        //    }
        //    else
        //        return Rest.Messages.Send(this, message);
        //}

        ///// <summary>
        ///// Sends a message with a file attachment to this channel.
        ///// </summary>
        ///// <param name="message">The message to send.</param>
        ///// <param name="file">The file data to attach.</param>
        //public DiscordMessage SendMessage(string message, byte[] file)
        //{
        //    return Rest.Messages.Send(this, message, file);
        //}
    }
}
