using Discore.Http;
using Discore.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore
{
    public sealed class DiscordGuildMember : DiscordIdEntity
    {
        /// <summary>
        /// Gets the id of the guild this member is in.
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
        /// Gets the ids of all of the roles this member has.
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

        DiscordHttpClient http;

        internal DiscordGuildMember(DiscordHttpClient http, MutableGuildMember member)
        {
            this.http = http;

            GuildId = member.GuildId;

            User = member.User.ImmutableEntity;

            Nickname = member.Nickname;
            JoinedAt = member.JoinedAt;
            IsDeaf = member.IsDeaf;
            IsMute = member.IsMute;

            RoleIds = new List<Snowflake>(member.RoleIds);
        }

        internal DiscordGuildMember(DiscordHttpClient http, DiscordApiData data, Snowflake guildId)
            // We do not specify the base constructor here because the member ID must be
            // manually retrieved, as it is actually the user id rather than a unique one.
        {
            this.http = http;

            GuildId = GuildId;

            Nickname = data.GetString("nick");
            JoinedAt = data.GetDateTime("joined_at").Value;
            IsDeaf = data.GetBoolean("deaf") ?? false;
            IsMute = data.GetBoolean("mute") ?? false;

            IList<DiscordApiData> rolesArray = data.GetArray("roles");
            Snowflake[] roleIds = new Snowflake[rolesArray.Count];

            for (int i = 0; i < rolesArray.Count; i++)
                roleIds[i] = rolesArray[i].ToSnowflake().Value;

            RoleIds = roleIds;

            DiscordApiData userData = data.Get("user");
            User = new DiscordUser(false, userData);

            Id = User.Id;
        }

        /// <summary>
        /// Modifies the attributes of this member.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task Modify(ModifyGuildMemberParameters parameters)
        {
            return http.ModifyGuildMember(GuildId, Id, parameters);
        }

        /// <summary>
        /// Removes this user from the guild they are a member of.
        /// <para>Requires <see cref="DiscordPermission.KickMembers"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task Kick()
        {
            return http.RemoveGuildMember(GuildId, Id);
        }

        /// <summary>
        /// Bans this user from the guild they are a member of.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <param name="deleteMessageDays">Number of days to delete messages for (0-7).</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task Ban(int? deleteMessageDays = null)
        {
            return http.CreateGuildBan(GuildId, Id, deleteMessageDays);
        }

        /// <summary>
        /// Adds a role to this member.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task AddRole(Snowflake roleId)
        {
            return http.AddGuildMemberRole(GuildId, Id, roleId);
        }

        /// <summary>
        /// Removes a role from this member.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task RemoveRole(Snowflake roleId)
        {
            return http.RemoveGuildMemberRole(GuildId, Id, roleId);
        }

        public override string ToString()
        {
            return Nickname != null ? $"{User.Username} aka. {Nickname}" : User.Username;
        }
    }
}
