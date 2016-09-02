using Discore.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Discore
{
    /// <summary>
    /// A guild member represents a <see cref="DiscordUser"/> that belongs to a <see cref="DiscordGuild"/>.
    /// </summary>
    public class DiscordGuildMember : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of the user.
        /// </summary>
        public string Id { get { return User?.Id; } }
        /// <summary>
        /// Gets the guild this member is in.
        /// </summary>
        public DiscordGuild Guild { get; private set; }
        /// <summary>
        /// Gets the actual user.
        /// </summary>
        public DiscordUser User { get; private set; }
        /// <summary>
        /// Gets the guild-wide nickname of the user.
        /// </summary>
        public string Nickname { get; private set; }
        /// <summary>
        /// Gets all the roles this member has.
        /// </summary>
        public DiscordRole[] Roles { get; private set; }
        /// <summary>
        /// Gets the time this member joined the guild.
        /// </summary>
        public DateTime JoinedAt { get; private set; }
        /// <summary>
        /// Gets the current voice state of this member.
        /// </summary>
        public DiscordVoiceState VoiceState { get; private set; }

        DiscordApiCache cache;

        /// <summary>
        /// Creates a new <see cref="DiscordGuildMember"/> instance.
        /// </summary>
        /// <param name="client">The associated <see cref="IDiscordClient"/>.</param>
        /// <param name="guild">The <see cref="DiscordGuild"/> this member belongs to.</param>
        public DiscordGuildMember(IDiscordClient client, DiscordGuild guild)
        {
            cache = client.Cache;

            Guild = guild;
            VoiceState = new DiscordVoiceState(client, this);
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
            for (int i = 0; i < Roles.Length; i++)
                userPermissions = userPermissions | Roles[i].Permissions;

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
            for (int i = 0; i < Roles.Length; i++)
            {
                DiscordRole role = Roles[i];

                userPermissions = userPermissions | role.Permissions;
            }

            // Administrator overrides channel-specific overwrites
            if (userPermissions.HasFlag(DiscordPermission.Administrator))
                return true;

            // Apply channel-specific overwrites
            for (int i = 0; i < Roles.Length; i++)
            {
                DiscordRole role = Roles[i];

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
            for (int i = 0; i < Roles.Length; i++)
                if (Roles[i].Name == roleName)
                    return true;

            return false;
        }

        /// <summary>
        /// Updates this guild member with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this guild member with.</param>
        public void Update(DiscordApiData data)
        {
            Nickname = data.GetString("nick") ?? Nickname;
            JoinedAt = data.GetDateTime("joined_at") ?? JoinedAt;

            // Update roles
            IList<DiscordApiData> rolesData = data.GetArray("roles");
            if (rolesData != null)
            {
                Roles = new DiscordRole[rolesData.Count];
                for (int i = 0; i < Roles.Length; i++)
                {
                    DiscordRole role;
                    string roleId = rolesData[i].ToString();
                    if (cache.TryGet(Guild, roleId, out role))
                        Roles[i] = role;
                    else
                        DiscordLogger.Default.LogWarning($"[GUILD_MEMBER.UPDATE:{data.LocateString("user.username")}] "
                            + $"Failed to locate role with id {roleId} in guild '{Guild.Name}'");
                }

                Roles.OrderBy(r => r.Position);
            }

            // Update user
            DiscordApiData userData = data.Get("user");
            if (userData != null)
            {
                string userId = userData.GetString("id");
                User = cache.AddOrUpdate(userId, userData, () => { return new DiscordUser(); });
            }

            User?.Update(data);

            // Update voice state
            VoiceState.UpdateFromGuildMemberUpdate(data);
        }

        #region Object Overrides
        /// <summary>
        /// Determines whether the specified <see cref="DiscordGuildMember"/> is equal 
        /// to the current member.
        /// </summary>
        /// <param name="other">The other <see cref="DiscordGuildMember"/> to check.</param>
        public bool Equals(DiscordGuildMember other)
        {
            return Id == other?.Id;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current member.
        /// </summary>
        /// <param name="obj">The other object to check.</param>
        public override bool Equals(object obj)
        {
            DiscordGuildMember other = obj as DiscordGuildMember;
            if (ReferenceEquals(other, null))
                return false;
            else
                return Equals(other);
        }

        /// <summary>
        /// Returns the hash of this member.
        /// </summary>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Gets the username of this member.
        /// </summary>
        public override string ToString()
        {
            return User.Username;
        }

#pragma warning disable 1591
        public static bool operator ==(DiscordGuildMember a, DiscordGuildMember b)
        {
            return a?.Id == b?.Id;
        }

        public static bool operator !=(DiscordGuildMember a, DiscordGuildMember b)
        {
            return a?.Id != b?.Id;
        }
#pragma warning restore 1591
        #endregion
    }
}
