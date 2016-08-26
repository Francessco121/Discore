using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Discore
{
    public abstract class DiscordChannel : IDiscordObject, ICacheable
    {
        public string Id { get; protected set; }
        public DiscordChannelType ChannelType { get; }

        public IReadOnlyDictionary<string, DiscordMessage> CachedMessages
        {
            get { return new ReadOnlyDictionary<string, DiscordMessage>(cachedMessages); }
        }

        protected IDiscordClient Client { get; }
        protected DiscordApiCache Cache { get; }

        ConcurrentDictionary<string, DiscordMessage> cachedMessages;
        List<string> cachedMessageIds;

        public DiscordChannel(IDiscordClient client, DiscordChannelType type) 
        {
            Client = client;
            Cache = client.Cache;
            ChannelType = type;

            cachedMessages = new ConcurrentDictionary<string, DiscordMessage>();
            cachedMessageIds = new List<string>();
        }

        public void Close()
        {
            Client.Rest.Channels.Close(this);
        }

        public bool TryGetMessage(string id, out DiscordMessage message)
        {
            return cachedMessages.TryGetValue(id, out message);
        }

        public bool TryRemoveMessage(string id, out DiscordMessage message)
        {
            lock (cachedMessageIds)
            {
                cachedMessageIds.Remove(id);
            }

            return cachedMessages.TryRemove(id, out message);
        }

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

        public bool CacheMessage(DiscordMessage message)
        {
            lock (cachedMessageIds)
            {
                cachedMessageIds.Add(message.Id);
            }

            return cachedMessages.TryAdd(message.Id, message);
        }

        public async void SendMessage(string message)
        {
            try
            {
                await Client.Rest.Messages.Send(this, message);
            }
            catch (Exception ex)
            {
                DiscordLogger.Default.LogError($"[DiscordChannel] {ex}");
            }
        }

        public async void SendMessage(string message, byte[] file)
        {
            try
            {
                await Client.Rest.Messages.Send(this, message, file);
            }
            catch (Exception ex)
            {
                DiscordLogger.Default.LogError($"[DiscordChannel] {ex}");
            }
        }

        public abstract void Update(DiscordApiData data);
    }
}
