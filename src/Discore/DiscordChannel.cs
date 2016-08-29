using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Discore
{
    /// <summary>
    /// A <see cref="DiscordDMChannel"/> or a <see cref="DiscordGuildChannel"/>.
    /// </summary>
    public abstract class DiscordChannel : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of this channel.
        /// </summary>
        public string Id { get; protected set; }
        /// <summary>
        /// Gets the type of this channel.
        /// </summary>
        public DiscordChannelType ChannelType { get; }

        /// <summary>
        /// Gets all cached <see cref="DiscordMessage"/>s in this channel.
        /// </summary>
        public IReadOnlyDictionary<string, DiscordMessage> CachedMessages
        {
            get { return new ReadOnlyDictionary<string, DiscordMessage>(cachedMessages); }
        }

        /// <summary>
        /// Gets the associated <see cref="IDiscordClient"/> with this channel.
        /// </summary>
        protected IDiscordClient Client { get; }
        /// <summary>
        /// Gets the cache of the associated <see cref="IDiscordClient"/>.
        /// </summary>
        protected DiscordApiCache Cache { get; }

        ConcurrentDictionary<string, DiscordMessage> cachedMessages;
        List<string> cachedMessageIds;

        /// <summary>
        /// Creates a new <see cref="DiscordChannel"/> instance.
        /// </summary>
        /// <param name="client">The associated <see cref="IDiscordClient"/>.</param>
        /// <param name="type">The type of channel.</param>
        public DiscordChannel(IDiscordClient client, DiscordChannelType type) 
        {
            Client = client;
            Cache = client.Cache;
            ChannelType = type;

            cachedMessages = new ConcurrentDictionary<string, DiscordMessage>();
            cachedMessageIds = new List<string>();
        }

        /// <summary>
        /// Closes the channel.
        /// </summary>
        public void Close()
        {
            Client.Rest.Channels.Close(this);
        }

        /// <summary>
        /// Attempts to get a <see cref="DiscordMessage"/> by its id.
        /// </summary>
        /// <param name="id">The id of the <see cref="DiscordMessage"/>.</param>
        /// <param name="message">The found <see cref="DiscordMessage"/>.</param>
        /// <returns>Returns whether or not the <see cref="DiscordMessage"/> was found.</returns>
        public bool TryGetMessage(string id, out DiscordMessage message)
        {
            return cachedMessages.TryGetValue(id, out message);
        }

        /// <summary>
        /// Attempts to remove a <see cref="DiscordMessage"/> by its id.
        /// </summary>
        /// <param name="id">The id of the <see cref="DiscordMessage"/>.</param>
        /// <param name="message">The deleted <see cref="DiscordMessage"/>.</param>
        /// <returns>Returns whether or not the <see cref="DiscordMessage"/> was deleted.</returns>
        public bool TryRemoveMessage(string id, out DiscordMessage message)
        {
            lock (cachedMessageIds)
            {
                cachedMessageIds.Remove(id);
            }

            return cachedMessages.TryRemove(id, out message);
        }

        /// <summary>
        /// Gets the last cached message ids in order.
        /// </summary>
        /// <param name="start">The start index to get messages.</param>
        /// <param name="limit">The maximum number of messages to return.</param>
        /// <returns>Returns the found message ids.</returns>
        public string[] GetMessageIds(int start, int limit)
        {
            List<string> ids = new List<string>();
            lock (cachedMessageIds)
            {
                for (int i = cachedMessageIds.Count - start - 1, e = 0; i >= 0 && e < limit; i--, e++)
                    ids.Add(cachedMessageIds[i]);

                return ids.ToArray();
            }
        }

        /// <summary>
        /// Caches a <see cref="DiscordMessage"/>.
        /// </summary>
        /// <param name="message">The <see cref="DiscordMessage"/> to cache.</param>
        /// <returns>Returns whether or not the <see cref="DiscordMessage"/> was cached.</returns>
        public bool CacheMessage(DiscordMessage message)
        {
            lock (cachedMessageIds)
            {
                cachedMessageIds.Add(message.Id);
            }

            return cachedMessages.TryAdd(message.Id, message);
        }

        /// <summary>
        /// Sends a message to this channel.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="splitIfTooLong">Whether to split this call into multiple messages if above 2000 characters.</param>
        /// <remarks>
        /// If <code>splitIfTooLong</code> is set to true, the message returned will be the first chunk sent.
        /// The message will be split by using newlines if possible to ensure the middle of a sentence isn't broken.
        /// </remarks>
        public async Task<DiscordMessage> SendMessage(string message, bool splitIfTooLong = false)
        {
            if (splitIfTooLong && message.Length > 2000)
            {
                DiscordMessage firstMsg = null;

                int i = 0;
                while (i < message.Length)
                {
                    int maxChars = Math.Min(2000, message.Length - i);
                    int lastNewLine = message.LastIndexOf('\n', i + maxChars - 1, maxChars - 1);

                    string subMessage;
                    if (lastNewLine > -1)
                        subMessage = message.Substring(i, lastNewLine - i);
                    else
                        subMessage = message.Substring(i, maxChars);

                    if (!string.IsNullOrWhiteSpace(subMessage))
                    {
                        DiscordMessage msg = await Client.Rest.Messages.Send(this, subMessage);

                        if (firstMsg == null)
                            firstMsg = msg;
                    }

                    i += subMessage.Length;
                }

                return firstMsg;
            }
            else
                return await Client.Rest.Messages.Send(this, message);
        }

        /// <summary>
        /// Sends a message with a file attachment to this channel.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="file">The file data to attach.</param>
        public async Task<DiscordMessage> SendMessage(string message, byte[] file)
        {
            return await Client.Rest.Messages.Send(this, message, file);
        }

        /// <summary>
        /// Updates this channel with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this channel with.</param>
        public abstract void Update(DiscordApiData data);

        #region Object Overrides
        /// <summary>
        /// Determines whether the specified <see cref="DiscordChannel"/> is equal 
        /// to the current channel.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordChannel"/> to check.</param>
        public bool Equals(DiscordChannel other)
        {
            return Id == other?.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current channel.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordChannel other = obj as DiscordChannel;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this DiscordChannel.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

#pragma warning disable 1591
        public static bool operator ==(DiscordChannel a, DiscordChannel b)
        {
            return a?.Id == b?.Id;
        }

        public static bool operator !=(DiscordChannel a, DiscordChannel b)
        {
            return a?.Id != b?.Id;
        }
#pragma warning restore 1591
        #endregion
    }
}
