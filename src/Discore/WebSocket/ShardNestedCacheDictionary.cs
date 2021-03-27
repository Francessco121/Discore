using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Discore.WebSocket
{
    class ShardNestedCacheDictionary<T>
        where T : class
    {
        ConcurrentDictionary<Snowflake, ShardCacheDictionary<T>> dictionary;

        public ShardNestedCacheDictionary()
        {
            dictionary = new ConcurrentDictionary<Snowflake, ShardCacheDictionary<T>>();
        }

        public T this[Snowflake parentId, Snowflake childId]
        {
            get
            {
                if (dictionary.TryGetValue(parentId, out ShardCacheDictionary<T> innerDictionary))
                {
                    if (innerDictionary.TryGetValue(childId, out T value))
                        return value;
                }

                return null;
            }
            set
            {
                ShardCacheDictionary<T> innerDictionary;
                if (!dictionary.TryGetValue(parentId, out innerDictionary))
                {
                    innerDictionary = new ShardCacheDictionary<T>();
                    dictionary[parentId] = innerDictionary;
                }

                innerDictionary[childId] = value;
            }
        }

        public bool TryGetValue(Snowflake parentId, Snowflake childId, out T value)
        {
            if (dictionary.TryGetValue(parentId, out ShardCacheDictionary<T> innerDictionary))
                return innerDictionary.TryGetValue(childId, out value);
            else
            {
                value = null;
                return false;
            }
        }

        public IEnumerable<T> GetValues(Snowflake parentId)
        {
            if (dictionary.TryGetValue(parentId, out ShardCacheDictionary<T> innerDictionary))
                return innerDictionary.Values;
            else
                return null;
        }

        public ShardCacheDictionary<T> GetOrCreateInner(Snowflake parentId)
        {
            ShardCacheDictionary<T> innerDictionary;
            if (!dictionary.TryGetValue(parentId, out innerDictionary))
            {
                innerDictionary = new ShardCacheDictionary<T>();
                dictionary[parentId] = innerDictionary;
            }

            return innerDictionary;
        }

        public bool TryRemove(Snowflake parentId, Snowflake childId, out T value)
        {
            if (dictionary.TryGetValue(parentId, out ShardCacheDictionary<T> innerDictionary))
                return innerDictionary.TryRemove(childId, out value);
            else
            {
                value = null;
                return false;
            }
        }

        public void RemoveParent(Snowflake parentId)
        {
            dictionary.TryRemove(parentId, out _);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public void Clear(Snowflake parentId)
        {
            if (dictionary.TryGetValue(parentId, out ShardCacheDictionary<T> innerDictionary))
                innerDictionary.Clear();
        }
    }
}
