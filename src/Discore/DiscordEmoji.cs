using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Discore
{
    public class DiscordEmoji : DiscordIdEntity
    {
        /// <summary>
        /// Gets the name of this emoji.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the IDs of associated roles with this emoji.
        /// </summary>
        public IReadOnlyList<Snowflake> RoleIds { get; }
        // TODO: Make full DiscordUser object
        /// <summary>
        /// Gets the ID of the user that created this emoji.
        /// </summary>
        public Snowflake? UserId { get; }
        /// <summary>
        /// Gets whether or not colons are required around the emoji name to use it.
        /// </summary>
        public bool RequireColons { get; }
        /// <summary>
        /// Gets whether or not this emoji is managed.
        /// </summary>
        public bool IsManaged { get; }
        /// <summary>
        /// Gets whether or not this emoji is animated.
        /// </summary>
        public bool IsAnimated { get; }

        // TODO: add available

        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="name"/> or <paramref name="roleIds"/> is null.
        /// </exception>
        public DiscordEmoji(
            Snowflake id,
            string name, 
            IReadOnlyList<Snowflake> roleIds, 
            Snowflake? userId, 
            bool requireColons, 
            bool isManaged, 
            bool isAnimated)
            : base(id)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            RoleIds = roleIds ?? throw new ArgumentNullException(nameof(roleIds));
            UserId = userId;
            RequireColons = requireColons;
            IsManaged = isManaged;
            IsAnimated = isAnimated;
        }

        internal DiscordEmoji(JsonElement json)
            : base(json)
        {
            Name = json.GetProperty("name").GetString()!;
            UserId = json.GetPropertyOrNull("user")?.GetProperty("id").GetSnowflake();
            RequireColons = json.GetPropertyOrNull("require_colons")?.GetBoolean() ?? false;
            IsManaged = json.GetPropertyOrNull("managed")?.GetBoolean() ?? false;
            IsAnimated = json.GetPropertyOrNull("animated")?.GetBoolean() ?? false;

            JsonElement? rolesJson = json.GetPropertyOrNull("roles");

            if (rolesJson != null)
            {
                JsonElement _rolesJson = rolesJson.Value;
                var roleIds = new Snowflake[_rolesJson.GetArrayLength()];

                for (int i = 0; i < roleIds.Length; i++)
                    roleIds[i] = _rolesJson[i].GetSnowflake();

                RoleIds = roleIds;
            }
            else
            {
                RoleIds = Array.Empty<Snowflake>();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
