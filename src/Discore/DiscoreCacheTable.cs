using System;
using System.Collections;
using System.Collections.Generic;

namespace Discore
{
    public class DiscoreCacheTable<T> : IDictionary<Snowflake, T>, IReadOnlyDictionary<Snowflake, T>, ICollection<T>, IEnumerable<KeyValuePair<Snowflake, T>>, IEnumerable
        where T : DiscordHashableObject
    {
        /* Reasoning for essentially encapsulating a hashtable:
         * 
         * 1) Consistency.
         * 2) We can't use a concurrent dictionary (see below), 
         *    but generic support is nice.
         * 3) Making it read-only to the public (could just use a ReadOnlyDictionary however,
         *    but we still need thread-saftey).
        */

        /* Reasoning for using a hashtable instead of a concurrent dictionary:
         * 
         * 1) ConcurrentDictionary has undefined behaviour for key and value enumeration,
         *    which is very often used.
         * 2) ConcurrentDictionary allows for multiple writers HOWEVER, we are guaranteed
         *    to only have one writer at a time, so the overhead is not worth it.
        */

        public ICollection<Snowflake> Keys
        {
            get
            {
                Snowflake[] keys;

                // Lock to ensure table count won't change
                // during the copy.
                lock (table.SyncRoot)
                {
                    keys = new Snowflake[table.Count];
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
        bool ICollection<KeyValuePair<Snowflake, T>>.IsReadOnly { get { throw new NotSupportedException(); } }

        IEnumerable<Snowflake> IReadOnlyDictionary<Snowflake, T>.Keys { get { return Keys; } }
        IEnumerable<T> IReadOnlyDictionary<Snowflake, T>.Values { get { return Values; } }

        Hashtable table;

        internal DiscoreCacheTable()
        {
            table = new Hashtable();
        }

        /// <summary>
        /// Gets an item by its ID, or null if the entry is not found.
        /// </summary>
        public T Get(Snowflake id)
        {
            return table[id] as T;
        }

        /// <summary>
        /// Gets a list of items by their ID's.
        /// Any entry not found will be null.
        /// </summary>
        public T[] Get(ICollection<Snowflake> ids)
        {
            T[] array = new T[ids.Count];

            int i = 0;
            foreach (Snowflake id in ids)
                array[i++] = Get(id);

            return array;
        }

        /// <summary>
        /// Gets an item by its ID, or null if the entry is not found.
        /// </summary>
        public T this[Snowflake id]
        {
            get { return Get(id); }
        }

        /// <summary>
        /// Gets a list of items by their ID's.
        /// Any entry not found will be null.
        /// </summary>
        public T[] this[ICollection<Snowflake> ids]
        {
            get { return Get(ids); }
        }

        /// <summary>
        /// Attempts to get an item by its id.
        /// </summary>
        public bool TryGetValue(Snowflake id, out T item)
        {
            item = table[id] as T;
            return item != null;
        }

        /// <summary>
        /// Returns whether this cache table contains the specified item.
        /// </summary>
        public bool Contains(T item)
        {
            return table.ContainsValue(item);
        }

        /// <summary>
        /// Returns whether this cache table contains the specified key.
        /// </summary>
        public bool ContainsKey(Snowflake key)
        {
            return table.ContainsKey(key);
        }

        /// <summary>
        /// Returns whether this cache table contains the specified item.
        /// </summary>
        public bool ContainsValue(T value)
        {
            return table.ContainsValue(value);
        }

        /// <summary>
        /// Copies all of the keys in this table to the specified array.
        /// </summary>
        /// <param name="arrayIndex">Position to start copying to in the specified array.</param>
        public void CopyTo(Snowflake[] array, int arrayIndex)
        {
            int i = arrayIndex;
            lock (table.SyncRoot)
            {
                foreach (Snowflake key in table.Keys)
                    array[i++] = key;
            }
        }

        /// <summary>
        /// Copies all of the values in this table to the specified array.
        /// </summary>
        /// <param name="arrayIndex">Position to start copying to in the specified array.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            int i = arrayIndex;
            lock (table.SyncRoot)
            {
                foreach (T value in table.Values)
                    array[i++] = value;
            }
        }

        /// <summary>
        /// Copies all of the entries in this table to the specified array.
        /// </summary>
        /// <param name="arrayIndex">Position to start copying to in the specified array.</param>
        public void CopyTo(KeyValuePair<Snowflake, T>[] array, int arrayIndex)
        {
            int i = arrayIndex;
            lock (table.SyncRoot)
            {
                foreach (DictionaryEntry entry in table)
                    array[i++] = new KeyValuePair<Snowflake, T>((Snowflake)entry.Key, (T)entry.Value);
            }
        }

        internal T Set(T value)
        {
            // Lock for thread-sensitive calls such as enumeration.
            lock (table.SyncRoot)
            {
                table[value.DictionaryId] = value;
            }

            return value;
        }

        internal T Remove(Snowflake id)
        {
            T obj;

            lock (table.SyncRoot)
            {
                obj = table[id] as T;
                table.Remove(id);
            }

            return obj;
        }

        internal void Clear()
        {
            lock (table.SyncRoot)
            {
                table.Clear();
            }
        }

        T IDictionary<Snowflake, T>.this[Snowflake key]
        {
            get { return this[key]; }
            set { throw new NotSupportedException(); }
        }

        public IEnumerator GetEnumerator()
        {
            // Make a copy of all entries for thread-saftey reasons.
            KeyValuePair<Snowflake, T>[] entries;

            lock (table.SyncRoot)
            {
                entries = new KeyValuePair<Snowflake, T>[table.Count];

                int i = 0;
                foreach (DictionaryEntry entry in table)
                    entries[i++] = new KeyValuePair<Snowflake, T>((Snowflake)entry.Key, (T)entry.Value);
            }

            return entries.GetEnumerator();
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

        IEnumerator<KeyValuePair<Snowflake, T>> IEnumerable<KeyValuePair<Snowflake, T>>.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        void IDictionary<Snowflake, T>.Add(Snowflake key, T value)
        {
            throw new NotSupportedException();
        }

        bool IDictionary<Snowflake, T>.Remove(Snowflake key)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<Snowflake, T>>.Add(KeyValuePair<Snowflake, T> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<Snowflake, T>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<Snowflake, T>>.Contains(KeyValuePair<Snowflake, T> item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<KeyValuePair<Snowflake, T>>.Remove(KeyValuePair<Snowflake, T> item)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}
