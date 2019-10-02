using System;
using System.Collections.Generic;

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
        public string Nickname { get; }

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

        internal DiscordMessageMember(DiscordApiData data)
        {
            Nickname = data.GetString("nick");
            JoinedAt = data.GetDateTime("joined_at").Value;
            IsDeaf = data.GetBoolean("deaf") ?? false;
            IsMute = data.GetBoolean("mute") ?? false;

            IList<DiscordApiData> rolesArray = data.GetArray("roles");
            if (rolesArray != null)
            {
                Snowflake[] roleIds = new Snowflake[rolesArray.Count];

                for (int i = 0; i < rolesArray.Count; i++)
                    roleIds[i] = rolesArray[i].ToSnowflake().Value;

                RoleIds = roleIds;
            }
        }
    }
}
