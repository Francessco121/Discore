using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Discore
{
    public class DiscordGuildMember : DiscordIdEntity, IDiscordGuildMember
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
        public string? Nickname { get; }

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

        // TODO: add premium_since, pending, permissions, communication_disabled_until

        internal DiscordGuildMember(
            Snowflake id,
            Snowflake guildId, 
            DiscordUser user, 
            string? nickname, 
            IReadOnlyList<Snowflake> roleIds, 
            DateTime joinedAt, 
            bool isDeaf, 
            bool isMute)
            : base(id)
        {
            GuildId = guildId;
            User = user ?? throw new ArgumentNullException(nameof(user));
            Nickname = nickname;
            RoleIds = roleIds ?? throw new ArgumentNullException(nameof(roleIds));
            JoinedAt = joinedAt;
            IsDeaf = isDeaf;
            IsMute = isMute;
        }

        internal DiscordGuildMember(JsonElement json, Snowflake guildId)
            : base(id: json.GetProperty("user").GetProperty("id").GetSnowflake())
        {
            GuildId = guildId;
            Nickname = json.GetPropertyOrNull("nick")?.GetString();
            JoinedAt = json.GetProperty("joined_at").GetDateTime();
            IsDeaf = json.GetProperty("deaf").GetBoolean();
            IsMute = json.GetProperty("mute").GetBoolean();

            JsonElement rolesJson = json.GetProperty("roles");
            var roles = new Snowflake[rolesJson.GetArrayLength()];

            for (int i = 0; i < roles.Length; i++)
                roles[i] = rolesJson[i].GetSnowflake();

            RoleIds = roles;

            User = new DiscordUser(json.GetProperty("user"), isWebhookUser: false);
        }

        public override string ToString()
        {
            return Nickname != null ? $"{User} aka. {Nickname}" : User.ToString();
        }
    }
}
