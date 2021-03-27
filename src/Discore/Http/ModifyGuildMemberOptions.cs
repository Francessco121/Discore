using System.Collections.Generic;

namespace Discore.Http
{
    /// <summary>
    /// An optional set of parameters to change the attributes of a guild member.
    /// </summary>
    public class ModifyGuildMemberOptions
    {
        /// <summary>
        /// Gets or sets the member's nickname for the guild (or null to leave unchanged).
        /// <para>Requires <see cref="DiscordPermission.ManageNicknames"/>.</para>
        /// </summary>
        public string Nickname { get; set; }
        /// <summary>
        /// Gets or sets a list of IDs for each role the member is to be assigned to (or null to leave unchanged).
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        public IEnumerable<Snowflake> RoleIds { get; set; }
        /// <summary>
        /// Gets or sets whether the member is server muted (or null to leave unchanged).
        /// <para>Requires <see cref="DiscordPermission.MuteMembers"/>.</para>
        /// </summary>
        public bool? IsServerMute { get; set; }
        /// <summary>
        /// Gets or sets whether the member is server deafened (or null to leave unchanged).
        /// <para>Requires <see cref="DiscordPermission.DeafenMembers"/>.</para>
        /// </summary>
        public bool? IsServerDeaf { get; set; }
        /// <summary>
        /// Gets or sets the ID of the voice channel to move the member to if they are currently connected to voice 
        /// (or null to leave unchanged).
        /// <para>The current bot must have permission to connect to this channel.</para>
        /// <para>Requires <see cref="DiscordPermission.MoveMembers"/>.</para>
        /// </summary>
        public Snowflake? ChannelId { get; set; }

        /// <summary>
        /// Sets the member's nickname for the guild.
        /// <para>Requires <see cref="DiscordPermission.ManageNicknames"/>.</para>
        /// </summary>
        public ModifyGuildMemberOptions SetNickname(string nickname)
        {
            Nickname = nickname;
            return this;
        }

        /// <summary>
        /// Sets the roles the member is to be assigned to.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <param name="roleIds">A list of IDs for each role the member is to be assigned to.</param>
        public ModifyGuildMemberOptions SetRoles(IEnumerable<Snowflake> roleIds)
        {
            RoleIds = roleIds;
            return this;
        }

        /// <summary>
        /// Sets whether the member is server mute.
        /// <para>Requires <see cref="DiscordPermission.MuteMembers"/>.</para>
        /// </summary>
        public ModifyGuildMemberOptions SetServerMute(bool isServerMute)
        {
            IsServerMute = isServerMute;
            return this;
        }

        /// <summary>
        /// Sets whether the member is server deafened.
        /// <para>Requires <see cref="DiscordPermission.DeafenMembers"/>.</para>
        /// </summary>
        public ModifyGuildMemberOptions SetServerDeaf(bool isServerDeaf)
        {
            IsServerDeaf = isServerDeaf;
            return this;
        }

        /// <summary>
        /// Sets the ID of the voice channel to move the member to (if they are currently connected to voice).
        /// <para>The current bot must have permission to connect to this channel.</para>
        /// <para>Requires <see cref="DiscordPermission.MoveMembers"/>.</para>
        /// </summary>
        public ModifyGuildMemberOptions SetVoiceChannel(Snowflake voiceChannelId)
        {
            ChannelId = voiceChannelId;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            if (Nickname != null)
                data.Set("nick", Nickname);
            if (IsServerMute.HasValue)
                data.Set("mute", IsServerMute);
            if (IsServerDeaf.HasValue)
                data.Set("deaf", IsServerDeaf);
            if (ChannelId.HasValue)
                data.SetSnowflake("channel_id", ChannelId);

            if (RoleIds != null)
            {
                DiscordApiData rolesArray = new DiscordApiData(DiscordApiDataType.Array);
                foreach (Snowflake roleId in RoleIds)
                    rolesArray.Values.Add(new DiscordApiData(roleId));

                data.Set("roles", rolesArray);
            }

            return data;
        }
    }
}
