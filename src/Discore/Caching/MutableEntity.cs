using ConcurrentCollections;
using System;

namespace Discore.Caching
{
    abstract class MutableEntity
    {
        public bool IsDirty { get; private set; }

        /// <summary>
        /// A set of all other mutable entities that contain a reference to this entity.
        /// </summary>
        readonly ConcurrentHashSet<MutableEntity> referencedBy;

        public MutableEntity()
        {
            referencedBy = new ConcurrentHashSet<MutableEntity>();
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void Reference(MutableEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.referencedBy.Add(this);
        }

        public void Dirty()
        {
            IsDirty = true;

            // Any entity that contains a reference to this entity also needs to be marked as dirty
            foreach (MutableEntity entity in referencedBy)
                entity.Dirty();
        }

        protected void ResetDirty()
        {
            IsDirty = false;
        }

        public void ClearReferences()
        {
            referencedBy.Clear();
        }
    }

    abstract class MutableEntity<T> : MutableEntity
        where T : class
    {
        public T ImmutableEntity
        {
            get
            {
                if (IsDirty || immutableEntity == null)
                {
                    immutableEntity = BuildImmutableEntity();
                    ResetDirty();
                }

                return immutableEntity;
            }
        }

        T? immutableEntity;

        protected abstract T BuildImmutableEntity();
    }
}
