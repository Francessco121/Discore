using System.Collections.Generic;

namespace Discore.Http
{
    /// <summary>
    /// An optional set of parameters to change the attributes of a guild member.
    /// </summary>
    public class ModifyGuildMemberParameters
    {
        /// <summary>
        /// Gets or sets the member's nickname for the guild (or null to leave unchanged).
        /// </summary>
        public string Nickname { get; set; }
        /// <summary>
        /// Gets or sets a list of IDs for each role the member is to be assigned to (or null to leave unchanged).
        /// </summary>
        public IEnumerable<Snowflake> RoleIds { get; set; }
        /// <summary>
        /// Gets or sets whether the member is server muted (or null to leave unchanged).
        /// </summary>
        public bool? IsServerMute { get; set; }
        /// <summary>
        /// Gets or sets whether the member is server deafened (or null to leave unchanged).
        /// </summary>
        public bool? IsServerDeaf { get; set; }
        /// <summary>
        /// Gets or sets the ID of the voice channel to move the member to if they are currently connected to voice 
        /// (or null to leave unchanged).
        /// </summary>
        public Snowflake? ChannelId { get; set; }

        /// <summary>
        /// Sets the member's nickname for the guild.
        /// </summary>
        public ModifyGuildMemberParameters SetNickname(string nickname)
        {
            Nickname = nickname;
            return this;
        }

        /// <summary>
        /// Sets the roles the member is to be assigned to.
        /// </summary>
        /// <param name="roleIds">A list of IDs for each role the member is to be assigned to.</param>
        public ModifyGuildMemberParameters SetRoles(IEnumerable<Snowflake> roleIds)
        {
            RoleIds = roleIds;
            return this;
        }

        /// <summary>
        /// Sets whether the member is server mute.
        /// </summary>
        public ModifyGuildMemberParameters SetServerMute(bool isServerMute)
        {
            IsServerMute = isServerMute;
            return this;
        }

        /// <summary>
        /// Sets whether the member is server deafened.
        /// </summary>
        public ModifyGuildMemberParameters SetServerDeaf(bool isServerDeaf)
        {
            IsServerDeaf = isServerDeaf;
            return this;
        }

        /// <summary>
        /// Sets the ID of the voice channel to move the member to (if they are currently connected to voice).
        /// </summary>
        public ModifyGuildMemberParameters SetVoiceChannel(Snowflake voiceChannelId)
        {
            ChannelId = voiceChannelId;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("nick", Nickname);
            data.Set("mute", IsServerMute);
            data.Set("deaf", IsServerDeaf);
            data.Set("channel_id", ChannelId);

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
