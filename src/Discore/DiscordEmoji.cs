using System.Collections.Generic;

namespace Discore
{
    /// <summary>
    /// A custom emoji for a <see cref="DiscordGuild"/>.
    /// </summary>
    public class DiscordEmoji : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the guild this emoji is for.
        /// </summary>
        public DiscordGuild Guild { get; private set; }
        /// <summary>
        /// Gets the id of this emoji.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the name of this emoji.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the associated roles with this emoji.
        /// </summary>
        public DiscordRole[] Roles { get; private set; }
        /// <summary>
        /// Gets whether or not colons are required around the emoji name to use it.
        /// </summary>
        public bool RequireColons { get; private set; }
        /// <summary>
        /// Gets whether or not this emoji is managed.
        /// </summary>
        public bool Managed { get; private set; }

        DiscordApiCache cache;

        /// <summary>
        /// Creates a new <see cref="DiscordEmoji"/> instance.
        /// </summary>
        /// <param name="client">The associated <see cref="IDiscordClient"/>.</param>
        /// <param name="guild">The <see cref="DiscordGuild"/> this emoji is for.</param>
        public DiscordEmoji(IDiscordClient client, DiscordGuild guild)
        {
            cache = client.Cache;
            Guild = guild;
        }

        /// <summary>
        /// Updates this emoji with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this emoji with.</param>
        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Name = data.GetString("name") ?? Name;
            RequireColons = data.GetBoolean("require_colons") ?? RequireColons;
            Managed = data.GetBoolean("managed") ?? Managed;

            IList<DiscordApiData> roles = data.GetArray("roles");
            if (roles != null)
            {
                Roles = new DiscordRole[roles.Count];
                for (int i = 0; i < Roles.Length; i++)
                {
                    string roleId = roles[i].ToString();
                    DiscordRole role;
                    if (cache.TryGet(Guild, roleId, out role))
                        Roles[i] = role;
                    else
                        DiscordLogger.Default.LogWarning($"[EMOJI.UPDATE] Failed to find role with id {roleId} in "
                            + $"guild '{Guild.Name}'");
                }
            }
        }

        #region Object Overrides
        /// <summary>
        /// Determines whether the specified <see cref="DiscordEmoji"/> is equal 
        /// to the current emoji.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordEmoji"/> to check.</param>
        public bool Equals(DiscordEmoji other)
        {
            return Id == other.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current emoji.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordEmoji other = obj as DiscordEmoji;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this emoji.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Gets the name of this emoji.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

#pragma warning disable 1591
        public static bool operator ==(DiscordEmoji a, DiscordEmoji b)
        {
            return a.Id == b.Id;
        }

        public static bool operator !=(DiscordEmoji a, DiscordEmoji b)
        {
            return a.Id != b.Id;
        }
#pragma warning restore 1591
        #endregion
    }
}
