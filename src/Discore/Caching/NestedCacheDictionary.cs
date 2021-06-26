using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Discore.Caching
{
    class NestedCacheDictionary<T>
        where T : class
    {
        readonly ConcurrentDictionary<Snowflake, CacheDictionary<T>> dictionary;

        public NestedCacheDictionary()
        {
            dictionary = new ConcurrentDictionary<Snowflake, CacheDictionary<T>>();
        }

        public T? this[Snowflake parentId, Snowflake childId]
        {
            get
            {
                if (dictionary.TryGetValue(parentId, out CacheDictionary<T>? innerDictionary))
                {
                    if (innerDictionary.TryGetValue(childId, out T? value))
                        return value;
                }

                return null;
            }
            set
            {
                CacheDictionary<T>? innerDictionary;
                if (!dictionary.TryGetValue(parentId, out innerDictionary))
                {
                    innerDictionary = new CacheDictionary<T>();
                    dictionary[parentId] = innerDictionary;
                }

                innerDictionary[childId] = value;
            }
        }

        public bool TryGetValue(Snowflake parentId, Snowflake childId, [NotNullWhen(true)] out T? value)
        {
            if (dictionary.TryGetValue(parentId, out CacheDictionary<T>? innerDictionary))
                return innerDictionary.TryGetValue(childId, out value);
            else
            {
                value = null;
                return false;
            }
        }

        public IEnumerable<T>? GetValues(Snowflake parentId)
        {
            if (dictionary.TryGetValue(parentId, out CacheDictionary<T>? innerDictionary))
                return innerDictionary.Values;
            else
                return null;
        }

        public CacheDictionary<T> GetOrCreateInner(Snowflake parentId)
        {
            CacheDictionary<T>? innerDictionary;
            if (!dictionary.TryGetValue(parentId, out innerDictionary))
            {
                innerDictionary = new CacheDictionary<T>();
                dictionary[parentId] = innerDictionary;
            }

            return innerDictionary;
        }

        public bool TryRemove(Snowflake parentId, Snowflake childId, [NotNullWhen(true)] out T? value)
        {
            if (dictionary.TryGetValue(parentId, out CacheDictionary<T>? innerDictionary))
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
            if (dictionary.TryGetValue(parentId, out CacheDictionary<T>? innerDictionary))
                innerDictionary.Clear();
        }
    }
}
