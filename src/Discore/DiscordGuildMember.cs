using System;
using System.Collections.Generic;

namespace Discore
{
    public sealed class DiscordGuildMember : DiscordIdEntity
    {
        /// <summary>
        /// Gets the ID of the guild this member is in.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// Gets the user data for this member.
        /// </summary>
        public DiscordUser User { get; }

        /// <summary>
        /// Gets the guild-wide nickname of the user.
        /// </summary>
        public string Nickname { get; }

        /// <summary>
        /// Gets the IDs of all of the roles this member has.
        /// </summary>
        public IReadOnlyList<Snowflake> RoleIds { get; }

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

        public DiscordGuildMember(
            Snowflake id,
            Snowflake guildId, 
            DiscordUser user, 
            string nickname, 
            IReadOnlyList<Snowflake> roleIds, 
            DateTime joinedAt, 
            bool isDeaf, 
            bool isMute)
            : base(id)
        {
            GuildId = guildId;
            User = user;
            Nickname = nickname;
            RoleIds = roleIds;
            JoinedAt = joinedAt;
            IsDeaf = isDeaf;
            IsMute = isMute;
        }

        internal static DiscordGuildMember FromJson(DiscordApiData data, Snowflake guildId)
        {
            IList<DiscordApiData> rolesArray = data.GetArray("roles");
            var roleIds = new Snowflake[rolesArray.Count];

            for (int i = 0; i < rolesArray.Count; i++)
                roleIds[i] = rolesArray[i].ToSnowflake().Value;

            DiscordApiData userData = data.Get("user");
            var user = new DiscordUser(false, userData);

            return new DiscordGuildMember(
                id: user.Id,
                guildId: guildId,
                user: user,
                nickname: data.GetString("nick"),
                roleIds: roleIds,
                joinedAt: data.GetDateTime("joined_at").Value,
                isDeaf: data.GetBoolean("deaf") ?? false,
                isMute: data.GetBoolean("mute") ?? false);
        }

        public override string ToString()
        {
            return Nickname != null ? $"{User.Username} aka. {Nickname}" : User.Username;
        }
    }
}
