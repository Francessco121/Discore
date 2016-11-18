using System;
using System.Collections.Generic;

namespace Discore.Http
{
    public class DiscordGuildMember : DiscordIdObject
    {
        /// <summary>
        /// Gets the id of the guild this member is in.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// Gets the actual user data for this member.
        /// </summary>
        public DiscordUser User { get; }

        /// <summary>
        /// Gets the guild-wide nickname of the user.
        /// </summary>
        public string Nickname { get; }

        /// <summary>
        /// Gets the ids of all of the roles this member has.
        /// </summary>
        public Snowflake[] RoleIds { get; }

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

        public DiscordGuildMember(DiscordApiData data, Snowflake guildId)
        {
            Nickname = data.GetString("nick");
            JoinedAt = data.GetDateTime("joined_at").Value;
            IsDeaf = data.GetBoolean("deaf").Value;
            IsMute = data.GetBoolean("mute").Value;

            // Get user
            DiscordApiData userData = data.Get("user");
            User = new DiscordUser(userData);

            Id = User.Id;

            // Get roles
            IList<DiscordApiData> rolesData = data.GetArray("roles");
            RoleIds = new Snowflake[rolesData.Count];

            for (int i = 0; i < rolesData.Count; i++)
                RoleIds[i] = rolesData[i].ToSnowflake().Value;
        }

        public override string ToString()
        {
            return Nickname != null ? $"{User.Username} aka. {Nickname}" : User.Username;
        }
    }
}
