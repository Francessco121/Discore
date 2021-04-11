using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Discore.WebSocket
{
    class MutableGuildMember : MutableEntity<DiscordGuildMember>
    {
        public MutableUser User { get; }
        public Snowflake GuildId { get; }

        public string? Nickname { get; private set; }
        public IReadOnlyList<Snowflake>? RoleIds { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public bool IsDeaf { get; private set; }
        public bool IsMute { get; private set; }

        public MutableGuildMember(MutableUser user, Snowflake guildId) 
        {
            User = user;
            Reference(user);

            GuildId = guildId;
        }

        public void Update(JsonElement json)
        {
            Nickname = json.GetPropertyOrNull("nick")?.GetString();
            JoinedAt = json.GetProperty("joined_at").GetDateTime();
            IsDeaf = json.GetProperty("deaf").GetBoolean();
            IsMute = json.GetProperty("mute").GetBoolean();

            RoleIds = GetRoles(json);

            Dirty();
        }

        public void PartialUpdate(JsonElement json)
        {
            Nickname = json.GetPropertyOrNull("nick")?.GetString() ?? Nickname;

            if (json.HasProperty("nick"))
                RoleIds = GetRoles(json);

            Dirty();
        }

        Snowflake[] GetRoles(JsonElement json)
        {
            JsonElement rolesJson = json.GetProperty("roles");
            var roles = new Snowflake[rolesJson.GetArrayLength()];

            for (int i = 0; i < roles.Length; i++)
                roles[i] = rolesJson[i].GetSnowflake();

            return roles;
        }

        protected override DiscordGuildMember BuildImmutableEntity()
        {
            return new DiscordGuildMember(
                id: User.Id,
                guildId: GuildId,
                user: User.ImmutableEntity,
                nickname: Nickname,
                roleIds: new List<Snowflake>(RoleIds!),
                joinedAt: JoinedAt,
                isDeaf: IsDeaf,
                isMute: IsMute);
        }
    }
}
