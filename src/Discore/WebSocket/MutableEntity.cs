using ConcurrentCollections;
using Discore.Http;
using System;

namespace Discore.WebSocket
{
    abstract class MutableEntity
    {
        public bool IsDirty { get; private set; }

        ConcurrentHashSet<MutableEntity> referencedBy;
        ConcurrentHashSet<MutableEntity> referencing;

        public MutableEntity()
        {
            referencedBy = new ConcurrentHashSet<MutableEntity>();
            referencing = new ConcurrentHashSet<MutableEntity>();
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void Reference(MutableEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            referencing.Add(entity);
            entity.referencedBy.Add(this);
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

        public void ClearReferences()
        {
            foreach (MutableEntity entity in referencedBy)
                entity.referencing.TryRemove(this);

            referencedBy.Clear();
            referencing.Clear();
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

        protected DiscordHttpClient Http { get; }

        T immutableEntity;

        public MutableEntity(DiscordHttpClient http)
        {
            Http = http;
        }

        protected abstract T BuildImmutableEntity();
    }
}
