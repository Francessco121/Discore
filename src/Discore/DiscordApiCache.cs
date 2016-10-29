using System;
using System.Collections;
using System.Collections.Generic;

namespace Discore
{
    public class DiscordApiCache
    {
        Hashtable cache;

        public DiscordApiCache()
        {
            cache = new Hashtable();
        }

        internal T Set<T>(DiscordApiData data, string id, Func<T> createCallback)
            where T : DiscordIdObject
        {
            Type objType = typeof(T);
            Hashtable typeTable = cache[objType] as Hashtable;
            if (typeTable == null)
            {
                typeTable = new Hashtable();
                cache[objType] = typeTable;
            }

            DiscordObject obj = typeTable[id] as DiscordObject;
            DiscordObject orig = obj;
            if (obj != null)
                obj = obj.MemberwiseClone();
            else
                obj = createCallback();

            obj.Update(data);
            typeTable[id] = obj;

            return (T)obj;
        }

        public T Get<T>(string id)
            where T : DiscordIdObject
        {
            Hashtable typeTable = cache[typeof(T)] as Hashtable;
            if (typeTable != null)
                return typeTable[id] as T;
            else
                return null;
        }

        public T[] Get<T>(ICollection<string> ids)
            where T : DiscordIdObject
        {
            Hashtable typeTable = cache[typeof(T)] as Hashtable;
            if (typeTable != null)
            {
                T[] array = new T[ids.Count];

                int i = 0;
                foreach (string id in ids)
                    array[i++] = typeTable[id] as T;

                return array;
            }
            else
                return new T[ids.Count];
        }

        internal void Clear()
        {
            cache.Clear();
        }
    }
}