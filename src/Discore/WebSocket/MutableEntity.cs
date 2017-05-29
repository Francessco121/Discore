using ConcurrentCollections;
using Discore.Http;
using System;

namespace Discore.WebSocket
{
    abstract class MutableEntity : IDisposable
    {
        public bool IsDirty { get; private set; }

        ConcurrentHashSet<MutableEntity> referencedBy;
        ConcurrentHashSet<MutableEntity> referencing;

        bool isDisposed;

        public MutableEntity()
        {
            referencedBy = new ConcurrentHashSet<MutableEntity>();
            referencing = new ConcurrentHashSet<MutableEntity>();
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void Reference(MutableEntity entity)
        {
            if (!isDisposed)
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                referencing.Add(entity);
                entity.referencedBy.Add(this);
            }
        }

        public void Dirty()
        {
            IsDirty = true;

            foreach (MutableEntity entity in referencedBy)
                entity.Dirty();
        }

        protected void ResetDirty()
        {
            IsDirty = false;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                foreach (MutableEntity entity in referencedBy)
                    entity.referencing.TryRemove(this);

                referencedBy.Clear();
                referencing.Clear();
            }
        }
    }

    abstract class MutableEntity<T> : MutableEntity
    {
        /// <summary>
        /// Note: Will return null if the immutable entity has not been built and the entity is not dirty.
        /// </summary>
        public T ImmutableEntity
        {
            get
            {
                if (IsDirty)
                {
                    immutableEntity = BuildImmutableEntity();
                    ResetDirty();
                }

                return immutableEntity;
            }
        }

        protected DiscordHttpApi Http { get; }

        T immutableEntity;

        public MutableEntity(DiscordHttpApi http)
        {
            Http = http;
        }

        protected abstract T BuildImmutableEntity();
    }
}
