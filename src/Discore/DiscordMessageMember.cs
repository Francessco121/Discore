using System;
using System.Collections.Generic;
using System.Text.Json;

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
