using System;

namespace Discore
{
    /// <summary>
    /// The base class for all Discord API objects that contain an id.
    /// </summary>
    public abstract class DiscordIdObject : DiscordObject
    {
        /// <summary>
        /// Gets the id of this Discord API object.
        /// </summary>
        public Snowflake Id { get; protected set; }

        internal DiscordIdObject() { }

        internal override void Update(DiscordApiData data)
        {
            Id = data.GetSnowflake("id") ?? Id;
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        /// <summary>
        /// Determines whether the specified <see cref="DiscordIdObject"/> is equal 
        /// to the current object.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordIdObject"/> to check.</param>
        public bool Equals(DiscordIdObject other)
        {
            return Id == other?.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordIdObject other = obj as DiscordIdObject;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this Discord API object.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(DiscordIdObject a, DiscordIdObject b)
        {
            return a?.Id == b?.Id;
        }

        public static bool operator !=(DiscordIdObject a, DiscordIdObject b)
        {
            return a?.Id != b?.Id;
        }
    }
}
