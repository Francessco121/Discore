using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Discore.Caching
{
    class CacheDictionary<T>
        where T : class
    {
        public int Count => dictionary.Count;

        public IEnumerable<T> Values => dictionary.Values;

        readonly ConcurrentDictionary<Snowflake, T> dictionary;

        public CacheDictionary()
        {
            dictionary = new ConcurrentDictionary<Snowflake, T>();
        }

        public T? this[Snowflake id]
        {
            get
            {
                if (dictionary.TryGetValue(id, out T? value))
                    return value;
                else
                    return null;
            }
            set
            {
                if (value == null)
                    dictionary.Remove(id, out _);
                else
                    dictionary[id] = value;
            }
        }

        public void AddRange(IEnumerable<KeyValuePair<Snowflake, T>> keyValuePairs)
        {
            foreach (KeyValuePair<Snowflake, T> pair in keyValuePairs)
                dictionary[pair.Key] = pair.Value;
        }

        public bool TryGetValue(Snowflake id, [NotNullWhen(true)] out T? value)
        {
            return dictionary.TryGetValue(id, out value);
        }

        public bool TryRemove(Snowflake id, [NotNullWhen(true)] out T? value)
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
