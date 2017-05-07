using Discore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore
{
    public sealed class DiscordGuildMember : DiscordIdObject
    {
        /// <summary>
        /// Gets the id of the guild this member is in.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// Gets the actual user data for this member.
        /// </summary>
        public DiscordUser User => cache != null ? cache.Users[Id] : user;

        /// <summary>
        /// Gets the guild-wide nickname of the user.
        /// </summary>
        public string Nickname { get; private set; }

        /// <summary>
        /// Gets the ids of all of the roles this member has.
        /// </summary>
        public IReadOnlyList<Snowflake> RoleIds { get; private set; }

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

        DiscordHttpGuildEndpoint guildsHttp;
        DiscoreCache cache;
        DiscordUser user;

        internal DiscordGuildMember(IDiscordApplication app, DiscoreCache cache, DiscordApiData data, Snowflake guildId)
            : this(app, data, guildId, true)
        {
            this.cache = cache;
        }

        internal DiscordGuildMember(IDiscordApplication app, DiscordApiData data, Snowflake guildId)
            : this(app, data, guildId, false)
        { }

        private DiscordGuildMember(IDiscordApplication app, DiscordApiData data, Snowflake guildId, bool isWebSocket)
            // We do not specify the base constructor here because the member ID must be
            // manually retrieved, as it is actually the user id rather than a unique one.
        {
            guildsHttp = app.HttpApi.Guilds;

            GuildId = guildId;

            Nickname = data.GetString("nick");
            JoinedAt = data.GetDateTime("joined_at").Value;
            IsDeaf = data.GetBoolean("deaf").Value;
            IsMute = data.GetBoolean("mute").Value;

            // Get roles
            IList<DiscordApiData> rolesArray = data.GetArray("roles");
            RoleIds = DeserializeRoleIds(rolesArray);

            if (!isWebSocket)
            {
                // Get user
                DiscordApiData userData = data.Get("user");
                user = new DiscordUser(userData);

                Id = User.Id;
            }
            else
                Id = data.LocateSnowflake("user.id").Value;
        }

        static Snowflake[] DeserializeRoleIds(IList<DiscordApiData> rolesArray)
        {
            Snowflake[] roleIds = new Snowflake[rolesArray.Count];

            for (int i = 0; i < rolesArray.Count; i++)
                roleIds[i] = rolesArray[i].ToSnowflake().Value;

            return roleIds;
        }

        internal DiscordGuildMember PartialUpdate(DiscordApiData updateData)
        {
            DiscordGuildMember newMember = (DiscordGuildMember)MemberwiseClone();
            newMember.RoleIds = DeserializeRoleIds(updateData.GetArray("roles"));
            newMember.Nickname = updateData.GetString("nick");

            return newMember;
        }

        /// <summary>
        /// Modifies the attributes of this member.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> Modify(ModifyGuildMemberParameters parameters)
        {
            return guildsHttp.ModifyMember(GuildId, Id, parameters);
        }

        /// <summary>
        /// Removes this user from the guild they are a member of.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> Kick()
        {
            return guildsHttp.RemoveMember(GuildId, Id);
        }

        /// <summary>
        /// Bans this user from the guild they are a member of.
        /// </summary>
        /// <param name="deleteMessageDays">Number of days to delete messages for (0-7).</param>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> Ban(int? deleteMessageDays = null)
        {
            return guildsHttp.CreateBan(GuildId, Id, deleteMessageDays);
        }

        /// <summary>
        /// Adds a role to this member.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> AddRole(Snowflake roleId)
        {
            return guildsHttp.AddMemberRole(GuildId, Id, roleId);
        }

        /// <summary>
        /// Removes a role from this member.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> RemoveRole(Snowflake roleId)
        {
            return guildsHttp.RemoveMemberRole(GuildId, Id, roleId);
        }

        public override string ToString()
        {
            return Nickname != null ? $"{User.Username} aka. {Nickname}" : User.Username;
        }
    }
}
