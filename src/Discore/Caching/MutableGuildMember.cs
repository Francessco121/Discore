using System;
using System.Collections.Generic;
using System.Linq;

namespace Discore.Caching
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

        public void Update(DiscordGuildMember member)
        {
            Nickname = member.Nickname;
            JoinedAt = member.JoinedAt;
            IsDeaf = member.IsDeaf;
            IsMute = member.IsMute;

            RoleIds = member.RoleIds.ToArray();

            Dirty();
        }

        public void PartialUpdate(DiscordPartialGuildMember member)
        {
            RoleIds = member.RoleIds.ToArray();
            Nickname = member.Nickname ?? Nickname;
            JoinedAt = member.JoinedAt ?? JoinedAt;
            IsDeaf = member.IsDeaf ?? IsDeaf;
            IsMute = member.IsMute ?? IsMute;

            Dirty();
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
