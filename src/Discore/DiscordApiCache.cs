using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Discore
{
    public class DiscordApiCache
    {
        public static DiscordApiCache Empty { get { return new DiscordApiCache(); } }

        ConcurrentDictionary<Type, ConcurrentDictionary<string, ICacheable>> cache;
        ConcurrentDictionary<Type, ConcurrentDictionary<string, DiscordApiCache>> innerCaches;

        public DiscordApiCache()
        {
            cache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, ICacheable>>();
            innerCaches = new ConcurrentDictionary<Type, ConcurrentDictionary<string, DiscordApiCache>>();
        }

        #region GetList
        public IReadOnlyList<KeyValuePair<string, T>> GetList<T>()
            where T : class, ICacheable
        {
            List<KeyValuePair<string, T>> list = new List<KeyValuePair<string, T>>();

            Type type = typeof(T);
            ConcurrentDictionary<string, ICacheable> typeCache;
            if (cache.TryGetValue(type, out typeCache))
            {
                foreach (KeyValuePair<string, ICacheable> pair in typeCache)
                    list.Add(new KeyValuePair<string, T>(pair.Key, (T)pair.Value));
            }

            return list;
        }

        public IReadOnlyList<KeyValuePair<string, T>> GetList<T, U>(U parentCacheable)
            where T : class, ICacheable
            where U : class, ICacheable
        {
            Type type = typeof(U);
            ConcurrentDictionary<string, DiscordApiCache> innerTypeCache;
            if (innerCaches.TryGetValue(type, out innerTypeCache))
            {
                DiscordApiCache innerCache;
                if (innerTypeCache.TryGetValue(parentCacheable.Id, out innerCache))
                    return innerCache.GetList<T>();
            }

            return new KeyValuePair<string, T>[0];
        }
        #endregion

        #region TryGet
        public bool TryGet<T>(string id, out T cacheable)
            where T : class, ICacheable
        {
            Type type = typeof(T);
            ConcurrentDictionary<string, ICacheable> typeCache;
            if (cache.TryGetValue(type, out typeCache))
            {
                ICacheable value;
                if (typeCache.TryGetValue(id, out value))
                {
                    cacheable = value as T;
                    return true;
                }
            }

            cacheable = null;
            return false;
        }

        public bool TryGet<T, U>(U parentCacheable, string childId, out T cacheable)
            where T : class, ICacheable
            where U : class, ICacheable
        {
            Type type = typeof(U);
            ConcurrentDictionary<string, DiscordApiCache> innerTypeCache;
            if (innerCaches.TryGetValue(type, out innerTypeCache))
            {
                DiscordApiCache innerCache;
                if (innerTypeCache.TryGetValue(parentCacheable.Id, out innerCache))
                    return innerCache.TryGet(childId, out cacheable);
            }

            cacheable = null;
            return false;
        }
        #endregion

        #region GetAndTryUpdate
        public bool GetAndTryUpdate<T>(string id, DiscordApiData data, out T cacheable)
            where T : class, ICacheable
        {
            Type type = typeof(T);
            ConcurrentDictionary<string, ICacheable> typeCache;
            if (cache.TryGetValue(type, out typeCache))
            {
                ICacheable existingCacheable;
                if (typeCache.TryGetValue(id, out existingCacheable))
                {
                    existingCacheable.Update(data);
                    cacheable = existingCacheable as T;
                    return true;
                }
            }

            cacheable = null;
            return false;
        }

        public bool GetAndTryUpdate<T, U>(U parentCacheable, string childId, DiscordApiData data, out T cacheable)
            where T : class, ICacheable
            where U : class, ICacheable
        {
            Type type = typeof(U);
            ConcurrentDictionary<string, DiscordApiCache> innerTypeCache;
            if (innerCaches.TryGetValue(type, out innerTypeCache))
            {
                DiscordApiCache innerCache;
                if (innerTypeCache.TryGetValue(parentCacheable.Id, out innerCache))
                    return innerCache.GetAndTryUpdate(childId, data, out cacheable);
            }

            cacheable = null;
            return false;
        }
        #endregion

        #region AddOrUpdate
        public T AddOrUpdate<T>(string id, DiscordApiData data, Func<T> createCallback)
            where T : class, ICacheable
        {
            Type type = typeof(T);
            ConcurrentDictionary<string, ICacheable> typeCache;
            if (!cache.TryGetValue(type, out typeCache))
            {
                // Type cache doesnt exist, so add it
                typeCache = new ConcurrentDictionary<string, ICacheable>();
                if (!cache.TryAdd(type, typeCache))
                    typeCache = cache[type];
            }

            ICacheable existingCacheable;
            if (!typeCache.TryGetValue(id, out existingCacheable))
            {
                // Object is not cached, so add it
                T cacheable = createCallback();
                if (!typeCache.TryAdd(id, cacheable))
                {
                    // Failed to add, so update the existing
                    ICacheable existing = typeCache[id];
                    existing.Update(data);
                    return existing as T;
                }
                else
                {
                    // Update the added one if successful
                    cacheable.Update(data);
                    return cacheable;
                }
            }
            else
            {
                // Update the existing
                existingCacheable.Update(data);
                return existingCacheable as T;
            }
        }

        public T AddOrUpdate<T, U>(U parentCacheable, string childId, DiscordApiData data, Func<T> createCallback, 
            bool makeGlobalAlias = false)
            where T : class, ICacheable
            where U : class, ICacheable
        {
            Type type = typeof(U);
            ConcurrentDictionary<string, DiscordApiCache> innerTypeCache;
            if (!innerCaches.TryGetValue(type, out innerTypeCache))
            {
                // Inner type cache doesnt exist, so add it
                innerTypeCache = new ConcurrentDictionary<string, DiscordApiCache>();
                if (!innerCaches.TryAdd(type, innerTypeCache))
                    innerTypeCache = innerCaches[type];
            }

            DiscordApiCache innerCache;
            if (!innerTypeCache.TryGetValue(parentCacheable.Id, out innerCache))
            {
                // Inner cache doesn't exist, so add it
                innerCache = new DiscordApiCache();
                if (!innerTypeCache.TryAdd(parentCacheable.Id, innerCache))
                {
                    // Failed to add, so use the existing
                    innerCache = innerTypeCache[parentCacheable.Id];
                }
            }

            T cacheable = innerCache.AddOrUpdate(childId, data, createCallback);

            if (makeGlobalAlias)
                SetAlias(cacheable);

            return cacheable;
        }
        #endregion

        #region TryRemove
        public bool TryRemove<T>(string id, out T cacheable)
            where T : class, ICacheable
        {
            Type type = typeof(T);
            ConcurrentDictionary<string, ICacheable> typeCache;
            if (cache.TryGetValue(type, out typeCache))
            {
                ICacheable existingCacheable;
                if (typeCache.TryGetValue(id, out existingCacheable))
                {
                    cacheable = existingCacheable as T;
                    return true;
                }
            }

            cacheable = null;
            return false;
        }

        public bool TryRemove<T, U>(U parentCacheable, string childId, out T cacheable)
            where T : class, ICacheable
            where U : class, ICacheable
        {
            Type type = typeof(U);
            ConcurrentDictionary<string, DiscordApiCache> innerTypeCache;
            if (!innerCaches.TryGetValue(type, out innerTypeCache))
            {
                cacheable = null;
                return false;
            }

            DiscordApiCache innerCache;
            if (!innerTypeCache.TryGetValue(parentCacheable.Id, out innerCache))
            {
                // Inner cache doesn't exist, so just try and remove alias
                return TryRemove(childId, out cacheable);
            }
            else
            {
                // Delete alias
                T temp;
                TryRemove(childId, out temp);

                // Delete cacheable from inner cache
                return innerCache.TryRemove(childId, out cacheable);
            }
        }
        #endregion

        #region Aliases
        public void SetAlias<T>(T cacheable)
            where T : class, ICacheable
        {
            Type type = typeof(T);
            ConcurrentDictionary<string, ICacheable> typeCache;
            if (!cache.TryGetValue(type, out typeCache))
            {
                // Type cache doesnt exist, so add it
                typeCache = new ConcurrentDictionary<string, ICacheable>();
                if (!cache.TryAdd(type, typeCache))
                    typeCache = cache[type];
            }

            ICacheable existingCacheable;
            if (!typeCache.TryGetValue(cacheable.Id, out existingCacheable))
            {
                // Object is not cached, so add it
                if (!typeCache.TryAdd(cacheable.Id, cacheable))
                {
                    // Failed to add, but the alias must be the same
                    // as the passed cacheable, so overwrite it.
                    typeCache[cacheable.Id] = cacheable;
                }
            }
            else
            {
                if (!object.ReferenceEquals(existingCacheable, cacheable))
                    // Existing alias differs from passed cacheable,
                    // so we will overwrite it.
                    typeCache[cacheable.Id] = cacheable;
            }
        }

        public bool TryRemoveAlias<T>(string id)
           where T : class, ICacheable
        {
            Type type = typeof(T);
            ConcurrentDictionary<string, ICacheable> typeCache;
            if (cache.TryGetValue(type, out typeCache))
            {
                ICacheable existingCacheable;
                if (typeCache.TryGetValue(id, out existingCacheable))
                    return true;
            }

            return false;
        }
        #endregion

        public void Clear()
        {
            cache.Clear();
            innerCaches.Clear();
        }
    }
}
