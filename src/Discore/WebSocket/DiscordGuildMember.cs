using Discore.WebSocket.Audio;
using System;
using System.Collections.Generic;

namespace Discore.WebSocket
{
    public sealed class DiscordGuildMember : DiscordIdObject
    {
        /// <summary>
        /// Gets the guild this member is in.
        /// </summary>
        public DiscordGuild Guild { get; private set; }

        /// <summary>
        /// Gets the actual user data for this member.
        /// </summary>
        public DiscordUser User { get; private set; }

        /// <summary>
        /// Gets the guild-wide nickname of the user.
        /// </summary>
        public string Nickname { get; private set; }

        /// <summary>
        /// Gets all the roles this member has.
        /// </summary>
        public DiscordApiCacheIdSet<DiscordRole> Roles { get; }

        /// <summary>
        /// Gets the time this member joined the guild.
        /// </summary>
        public DateTime JoinedAt { get; private set; }

        /// <summary>
        /// Gets the current voice state for this member.
        /// </summary>
        public DiscordVoiceState VoiceState { get; private set; }

        Shard shard;

        internal DiscordGuildMember(Shard shard, DiscordGuild guild)
        {
            this.shard = shard;
            Guild = guild;

            VoiceState = new DiscordVoiceState(shard, this);
            Roles = new DiscordApiCacheIdSet<DiscordRole>(Guild.Roles);
        }

        /// <summary>
        /// Gets whether or not this member has the specified permissions.
        /// </summary>
        /// <param name="permission">The permissions to check.</param>
        /// <returns>Returns whether or not the member has permission.</returns>
        public bool HasPermission(DiscordPermission permission)
        {
            // Calculate permissions from member roles
            DiscordPermission userPermissions = 0;
            foreach (DiscordRole role in Roles)
                userPermissions = userPermissions | role.Permissions;

            // Check for permission
            return userPermissions.HasFlag(DiscordPermission.Administrator) || userPermissions.HasFlag(permission);
        }

        /// <summary>
        /// Gets whether or not this member has the specified permissions in the context of
        /// the specified <see cref="DiscordGuildChannel"/>.
        /// </summary>
        /// <param name="permission">The permissions to check.</param>
        /// <param name="forChannel">The channel to check permissions for.</param>
        /// <returns>Returns whether or not the member has permission.</returns>
        /// <exception cref="ArgumentException">Thrown if the guild channel is not in the same guild as this member.</exception>
        public bool HasPermission(DiscordPermission permission, DiscordGuildChannel forChannel)
        {
            if (forChannel.Guild != Guild)
                throw new ArgumentException("Guild channel must be in the same guild as this member");

            // If owner, everything is true
            if (Id == forChannel.Guild.Owner.Id)
                return true;

            // Set default permissions to guild @everyone role permissions
            DiscordPermission userPermissions = forChannel.Guild.AtEveryoneRole.Permissions;

            // Apply guild-member role permissions
            foreach (DiscordRole role in Roles)
                userPermissions = userPermissions | role.Permissions;

            // Administrator overrides channel-specific overwrites
            if (userPermissions.HasFlag(DiscordPermission.Administrator))
                return true;

            // Apply channel @everyone overwrites
            DiscordOverwrite channelEveryoneOverwrite;
            if (forChannel.RolePermissionOverwrites.TryGetValue(forChannel.Guild.AtEveryoneRole.Id, out channelEveryoneOverwrite))
            {
                userPermissions = (userPermissions | channelEveryoneOverwrite.Allow) & (~channelEveryoneOverwrite.Deny);
            }

            // Apply channel-specific overwrites
            foreach (DiscordRole role in Roles)
            {
                DiscordOverwrite overwrite;
                if (forChannel.RolePermissionOverwrites.TryGetValue(role.Id, out overwrite))
                {
                    userPermissions = (userPermissions | overwrite.Allow) & (~overwrite.Deny);
                }
            }

            // Apply channel-specific member overwrite for this channel
            DiscordOverwrite memberOverwrite;
            if (forChannel.MemberPermissionOverwrites.TryGetValue(User.Id, out memberOverwrite))
            {
                userPermissions = userPermissions & (~memberOverwrite.Deny) | memberOverwrite.Allow;
            }

            // Check for correct permissions
            return userPermissions.HasFlag(DiscordPermission.Administrator) | userPermissions.HasFlag(permission);
        }

        /// <summary>
        /// Checks if this member has the specified permissions,
        /// if not a <see cref="DiscordPermissionException"/> is thrown.
        /// </summary>
        /// <param name="permission">The permissions to check.</param>
        /// <exception cref="DiscordPermissionException">Thrown if the member does not have the specified permissions.</exception>
        public void AssertPermission(DiscordPermission permission)
        {
            if (!HasPermission(permission))
                throw new DiscordPermissionException(this, permission);
        }

        /// <summary>
        /// Checks if this member has the specified permissions in the context 
        /// of the specified <see cref="DiscordGuildChannel"/>,
        /// if not a <see cref="DiscordPermissionException"/> is thrown.
        /// </summary>
        /// <param name="permission">The permissions to check.</param>
        /// <param name="channel">The channel to check permissions for.</param>
        /// <exception cref="DiscordPermissionException">Thrown if the member does not have the specified permissions.</exception>
        public void AssertPermission(DiscordPermission permission, DiscordGuildChannel channel)
        {
            if (!HasPermission(permission, channel))
                throw new DiscordPermissionException(this, channel, permission);
        }

        /// <summary>
        /// Gets whether or not this member has the 
        /// specified role by its name.
        /// </summary>
        /// <param name="roleName">The name of the role to check for.</param>
        /// <returns>Returns whether or not this member has the specified role.</returns>
        public bool HasRole(string roleName)
        {
            foreach (DiscordRole role in Roles)
                if (role.Name == roleName)
                    return true;

            return false;
        }

        internal override void Update(DiscordApiData data)
        {
            // Skip base.Update(data) here as the id is not at the root
            // of the guild member data, it is user.id.

            Nickname = data.GetString("nick") ?? Nickname;
            JoinedAt = data.GetDateTime("joined_at") ?? JoinedAt;

            // Get user
            DiscordApiData userData = data.Get("user");
            if (userData != null)
            {
                Id = userData.GetSnowflake("id").Value;

                User = shard.Users.Edit(Id, 
                    () => new DiscordUser(),
                    user => user.Update(userData));
            }

            // Get roles
            IList<DiscordApiData> rolesData = data.GetArray("roles");
            if (rolesData != null)
            {
                Roles.Clear();
                for (int i = 0; i < rolesData.Count; i++)
                {
                    Snowflake roleId = rolesData[i].ToSnowflake().Value;
                    Roles.Add(roleId);
                }
            }

            // Update voice state
            VoiceState.UpdateFromGuildMemberUpdate(data);
        }

        public override string ToString()
        {
            return Nickname != null ? $"{User.Username} aka. {Nickname}" : User.Username;
        }
    }
}
