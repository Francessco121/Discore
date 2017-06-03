using Discore.Http;
using System;
using System.Collections.Generic;

namespace Discore.WebSocket
{
    class MutableGuildMember : MutableEntity<DiscordGuildMember>
    {
        public MutableUser User { get; }
        public Snowflake GuildId { get; }

        public string Nickname { get; private set; }
        public IReadOnlyList<Snowflake> RoleIds { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public bool IsDeaf { get; private set; }
        public bool IsMute { get; private set; }

        public MutableGuildMember(MutableUser user, Snowflake guildId, DiscordHttpApi http) 
            : base(http)
        {
            User = user;
            Reference(user);

            GuildId = guildId;
        }

        public void Update(DiscordApiData data)
        {
            Nickname = data.GetString("nick");
            JoinedAt = data.GetDateTime("joined_at").Value;
            IsDeaf = data.GetBoolean("deaf") ?? false;
            IsMute = data.GetBoolean("mute") ?? false;

            UpdateRoles(data);

            Dirty();
        }

        public void PartialUpdate(DiscordApiData data)
        {
            Nickname = data.GetString("nick") ?? Nickname;

            if (data.ContainsKey("roles"))
                UpdateRoles(data);

            Dirty();
        }

        void UpdateRoles(DiscordApiData data)
        {
            IList<DiscordApiData> rolesArray = data.GetArray("roles");
            Snowflake[] roleIds = new Snowflake[rolesArray.Count];

            for (int i = 0; i < rolesArray.Count; i++)
                roleIds[i] = rolesArray[i].ToSnowflake().Value;

            RoleIds = roleIds;
        }

        protected override DiscordGuildMember BuildImmutableEntity()
        {
            throw new NotImplementedException();
        }
    }
}
