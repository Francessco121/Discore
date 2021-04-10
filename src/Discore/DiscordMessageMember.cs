using System;
using System.Collections.Generic;
using System.Text.Json;

#nullable enable

namespace Discore
{
    public class DiscordMessageMember
    {
        /// <summary>
        /// Gets the IDs of all of the roles this member has.
        /// </summary>
        public IReadOnlyList<Snowflake> RoleIds { get; }

        /// <summary>
        /// Gets the guild-wide nickname of the user.
        /// </summary>
        public string? Nickname { get; }

        /// <summary>
        /// Gets the time this member joined the guild.
        /// </summary>
        public DateTime JoinedAt { get; }

        /// <summary>
        /// Gets whether this member is deafened.
        /// </summary>
        public bool IsDeaf { get; }

        /// <summary>
        /// Gets whether this member is muted.
        /// </summary>
        public bool IsMute { get; }

        // TODO: add pending

        /// <exception cref="ArgumentNullException">Thrown if <paramref name="roleIds"/> is null.</exception>
        public DiscordMessageMember(
            IReadOnlyList<Snowflake> roleIds, 
            string? nickname, 
            DateTime joinedAt,
            bool isDeaf, 
            bool isMute)
        {
            RoleIds = roleIds ?? throw new ArgumentNullException(nameof(roleIds));
            Nickname = nickname;
            JoinedAt = joinedAt;
            IsDeaf = isDeaf;
            IsMute = isMute;
        }

        internal DiscordMessageMember(JsonElement json)
        {
            Nickname = json.GetPropertyOrNull("nick")?.GetString();
            JoinedAt = json.GetProperty("joined_at").GetDateTime();
            IsDeaf = json.GetProperty("deaf").GetBoolean();
            IsMute = json.GetProperty("mute").GetBoolean();

            JsonElement rolesJson = json.GetProperty("roles");
            var roles = new Snowflake[rolesJson.GetArrayLength()];

            for (int i = 0; i < roles.Length; i++)
                roles[i] = rolesJson[i].GetSnowflake();

            RoleIds = roles;
        }
    }
}

#nullable restore
