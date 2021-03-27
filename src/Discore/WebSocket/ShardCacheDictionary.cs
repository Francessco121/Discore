using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Discore.WebSocket
{
    class ShardCacheDictionary<T>
        where T : class
    {
        public int Count => dictionary.Count;

        public IEnumerable<T> Values => dictionary.Values;

        ConcurrentDictionary<Snowflake, T> dictionary;

        public ShardCacheDictionary()
        {
            dictionary = new ConcurrentDictionary<Snowflake, T>();
        }

        public T this[Snowflake id]
        {
            get
            {
                if (dictionary.TryGetValue(id, out T value))
                    return value;
                else
                    return null;
            }
            set => dictionary[id] = value;
        }

        public bool TryGetValue(Snowflake id, out T value)
        {
            return dictionary.TryGetValue(id, out value);
        }

        public bool TryRemove(Snowflake id, out T value)
        {
            return dictionary.TryRemove(id, out value);
        }

        public IReadOnlyDictionary<Snowflake, T> CreateReadonlyCopy()
        {
            return new Dictionary<Snowflake, T>(dictionary);
        }

        public void Clear()
        {
            dictionary.Clear();
        }
    }
}
