using System;

namespace Discore
{
    /// <summary>
    /// The base class for all Discord entities that contain an ID.
    /// </summary>
    public abstract class DiscordIdEntity
    {
        /// <summary>
        /// Gets the ID of this Discord entity.
        /// </summary>
        public Snowflake Id { get; protected set; }

        [Obsolete]
        internal DiscordIdEntity() { }

        internal DiscordIdEntity(Snowflake id) 
        { 
            Id = id; 
        }

        internal DiscordIdEntity(DiscordApiData data)
        {
            Id = data.GetSnowflake("id").Value;
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        /// <summary>
        /// Determines whether the specified <see cref="DiscordIdEntity"/> is equal to the current entity.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordIdEntity"/> to check.</param>
        public bool Equals(DiscordIdEntity other)
        {
            return Id == other?.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current Discord entity.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordIdEntity other = obj as DiscordIdEntity;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this Discord entity.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(DiscordIdEntity a, DiscordIdEntity b)
        {
            return a?.Id == b?.Id;
        }

        public static bool operator !=(DiscordIdEntity a, DiscordIdEntity b)
        {
            return a?.Id != b?.Id;
        }
    }
}
