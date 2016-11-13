using System;
using System.Collections;
using System.Collections.Generic;

namespace Discore
{
    public class DiscordApiCacheIdSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable
        where T : DiscordIdObject
    {
        class Enumerator : IEnumerator<T>, IEnumerator
        {
            public T Current
            {
                get
                {
                    string id = enumerator.Current;
                    return table.Get(id);
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            DiscordApiCacheTable<T> table;
            IEnumerator<string> enumerator;

            public Enumerator(DiscordApiCacheTable<T> table, IEnumerable<string> keys)
            {
                this.table = table;
                enumerator = keys.GetEnumerator();
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Reset();
            }

            public void Dispose()
            {
                enumerator.Dispose();
            }
        }

        public int Count { get { return set.Count; } }

        bool ICollection<T>.IsReadOnly { get { throw new NotSupportedException(); } }

        DiscordApiCacheTable<T> table;
        HashSet<string> set;

        internal DiscordApiCacheIdSet(DiscordApiCacheTable<T> table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            this.table = table;
            set = new HashSet<string>();
        }

        public T this[string id]
        {
            get { return Get(id); }
        }

        public T Get(string id)
        {
            if (set.Contains(id))
                return table.Get(id);
            else
                return null;
        }

        public bool TryGetValue(string id, out T item)
        {
            if (set.Contains(id))
            {
                item = table.Get(id);
                return item != null;
            }
            else
            {
                item = null;
                return false;
            }
        }

        public bool Contains(string id)
        {
            lock (set)
            {
                return set.Contains(id);
            }
        }

        public bool Contains(T item)
        {
            lock (set)
            {
                return set.Contains(item.Id);
            }
        }

        /// <summary>
        /// Copies all id's in this set to the specified array.
        /// </summary>
        public void CopyTo(string[] array, int arrayIndex)
        {
            int i = arrayIndex;
            lock (set)
            {
                foreach (string id in set)
                    array[i++] = id;
            }
        }

        /// <summary>
        /// Copies all items in this set to the specified array.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            int i = arrayIndex;
            lock (set)
            {
                foreach (string id in set)
                    array[i++] = table.Get(id);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            string[] keys;

            lock (set)
            {
                keys = new string[set.Count];
                int i = 0;
                foreach (string key in set)
                    keys[i++] = key;
            }

            return new Enumerator(table, keys);
        }

        internal void Add(string id)
        {
            lock (set)
            {
                set.Add(id);
            }
        }

        internal void Remove(string id)
        {
            lock (set)
            {
                set.Remove(id);
            }
        }

        internal void Clear()
        {
            lock (set)
            {
                set.Clear();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region Unsupported Methods
        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}