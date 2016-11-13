using System;
using System.Collections;
using System.Collections.Generic;

namespace Discore
{
    public class DiscordApiCacheTable<T> : IDictionary<string, T>, ICollection<T>, IEnumerable<KeyValuePair<string, T>>, IEnumerable
        where T : DiscordIdObject
    {
        /* Reasoning for essentially encapsulating a hashtable:
         * 
         * 1) Consistency.
         * 2) We can't use a concurrent dictionary (see below), 
         *    but generic support is nice.
         * 3) Making it read-only to the public (could just use a ReadOnlyDictionary however).
        */

        /* Reasoning for using a hashtable instead of a concurrent dictionary:
         * 
         * 1) ConcurrentDictionary has undefined behaviour for key and value enumeration,
         *    which is very often used.
         * 2) ConcurrentDictionary allows for multiple writers HOWEVER, we are guaranteed
         *    to only have one writer at a time, so the overhead is not worth it.
        */

        public ICollection<string> Keys
        {
            get
            {
                string[] keys;

                // Lock to ensure table count won't change
                // during the copy.
                lock (table.SyncRoot)
                {
                    keys = new string[table.Count];
                    table.Keys.CopyTo(keys, 0);
                }

                return keys;
            }
        }

        public ICollection<T> Values
        {
            get
            {
                T[] values;

                // Lock to ensure table count won't change
                // during the copy.
                lock (table.SyncRoot)
                {
                    values = new T[table.Count];
                    table.Values.CopyTo(values, 0);
                }

                return values;
            }
        }

        public int Count
        {
            get { return table.Count; }
        }

        bool ICollection<T>.IsReadOnly { get { throw new NotSupportedException(); } }
        bool ICollection<KeyValuePair<string, T>>.IsReadOnly { get { throw new NotSupportedException(); } }

        Hashtable table;

        internal DiscordApiCacheTable()
        {
            table = new Hashtable();
        }

        /// <summary>
        /// Gets an item by its id, or null if the entry is not found.
        /// </summary>
        public T Get(string id)
        {
            return table[id] as T;
        }

        public T this[string id]
        {
            get { return Get(id); }
        }

        /// <summary>
        /// Attempts to get an item by its id.
        /// </summary>
        public bool TryGetValue(string id, out T item)
        {
            item = table[id] as T;
            return item != null;
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            // Make a copy of all entries for thread-saftey reasons.
            KeyValuePair<string, T>[] entries;

            lock (table.SyncRoot)
            {
                entries = new KeyValuePair<string, T>[table.Count];

                int i = 0;
                foreach (DictionaryEntry entry in table)
                    entries[i++] = new KeyValuePair<string, T>((string)entry.Key, (T)entry.Value);
            }

            return (IEnumerator<KeyValuePair<string, T>>)entries.GetEnumerator();
        }

        public bool Contains(T item)
        {
            return table.ContainsValue(item);
        }

        public bool ContainsKey(string key)
        {
            return table.ContainsKey(key);
        }

        public bool ContainsValue(T value)
        {
            return table.ContainsValue(value);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            int i = arrayIndex;
            lock (table.SyncRoot)
            {
                foreach (string key in table.Keys)
                    array[i++] = key;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int i = arrayIndex;
            lock (table.SyncRoot)
            {
                foreach (T value in table.Values)
                    array[i++] = value;
            }
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            int i = arrayIndex;
            lock (table.SyncRoot)
            {
                foreach (DictionaryEntry entry in table)
                    array[i++] = new KeyValuePair<string, T>((string)entry.Key, (T)entry.Value);
            }
        }

        internal void Set(string id, T value)
        {
            // Lock for thread-sensitive calls such as enumeration.
            lock (table.SyncRoot)
            {
                table[id] = value;
            }
        }

        /// <summary>
        /// Provides a thread-safe way to update/add an item in the table.
        /// </summary>
        internal T Edit(string id, Func<T> createCallback, Action<T> editCallback)
        {
            T item = table[id] as T;
            if (item == null)
            {
                item = createCallback();
                editCallback(item);

                // Lock for thread-sensitive calls such as enumeration.
                lock (table.SyncRoot)
                {
                    table[id] = item;
                }
            }
            else
                editCallback(item);

            return item;
        }

        internal void Clear()
        {
            lock (table.SyncRoot)
            {
                table.Clear();
            }
        }

        T IDictionary<string, T>.this[string key]
        {
            get { return this[key]; }
            set { throw new NotSupportedException(); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Unsupported Methods
        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        void IDictionary<string, T>.Add(string key, T value)
        {
            throw new NotSupportedException();
        }

        bool IDictionary<string, T>.Remove(string key)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<string, T>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}
