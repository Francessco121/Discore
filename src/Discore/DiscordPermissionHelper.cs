using System;

namespace Discore
{
    public static class DiscordPermissionHelper
    {
        /// <summary>
        /// Gets whether a member has a set of permissions.
        /// </summary>
        /// <param name="permission">The set of permissions to check if the member has.</param>
        /// <param name="member">The member to check the permissions of.</param>
        /// <param name="guild">The guild this member is in.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool HasPermission(DiscordPermission permission, DiscordGuildMember member, DiscordGuild guild)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            if (guild == null)
                throw new ArgumentNullException(nameof(guild));

            if (member.GuildId != guild.Id)
                throw new ArgumentException("Member must be in the specified guild.");

            // Calculate permissions from member roles
            DiscordPermission userPermissions = 0;
            foreach (DiscordRole role in guild.Roles.Values)
                userPermissions = userPermissions | role.Permissions;

            // Check for permission
            return userPermissions.HasFlag(DiscordPermission.Administrator) || userPermissions.HasFlag(permission);
        }

        /// <summary>
        /// Gets whether a member has a set of permissions, 
        /// in the context of the specified guild channel.
        /// </summary>
        /// <param name="permission">The set of permissions to check if the member has.</param>
        /// <param name="member">The member to check the permissions of.</param>
        /// <param name="guild">The guild this member is in.</param>
        /// <param name="forChannel">
        /// The guild channel to check the permissions against (this will take overwrites into account).
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool HasPermission(DiscordPermission permission, 
            DiscordGuildMember member, DiscordGuild guild, DiscordGuildChannel forChannel)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            if (guild == null)
                throw new ArgumentNullException(nameof(guild));
            if (forChannel == null)
                throw new ArgumentNullException(nameof(forChannel));

            if (forChannel.GuildId != member.GuildId)
                throw new ArgumentException("Guild channel must be in the same guild as this member.");
            if (forChannel.GuildId != guild.Id)
                throw new ArgumentException("Guild channel must be in the specified guild.");

            // If owner, everything is true
            if (member.Id == guild.OwnerId)
                return true;

            // Set default permissions to guild @everyone role permissions
            DiscordPermission userPermissions = guild.Roles[guild.Id].Permissions;

            // Apply guild-member role permissions
            foreach (Snowflake roleId in member.RoleIds)
            {
                DiscordRole role;
                if (guild.Roles.TryGetValue(roleId, out role))
                {
                    userPermissions = userPermissions | role.Permissions;
                }
            }

            // Administrator overrides channel-specific overwrites
            if (userPermissions.HasFlag(DiscordPermission.Administrator))
                return true;

            // Apply channel @everyone overwrites
            DiscordOverwrite channelEveryoneOverwrite;
            if (forChannel.PermissionOverwrites.TryGetValue(guild.Id, out channelEveryoneOverwrite))
            {
                userPermissions = (userPermissions | channelEveryoneOverwrite.Allow) & (~channelEveryoneOverwrite.Deny);
            }

            // Apply channel-specific overwrites
            foreach (Snowflake roleId in member.RoleIds)
            {
                DiscordOverwrite overwrite;
                if (forChannel.PermissionOverwrites.TryGetValue(roleId, out overwrite))
                {
                    userPermissions = (userPermissions | overwrite.Allow) & (~overwrite.Deny);
                }
            }

            // Apply channel-specific member overwrite for this channel
            DiscordOverwrite memberOverwrite;
            if (forChannel.PermissionOverwrites.TryGetValue(member.Id, out memberOverwrite))
            {
                userPermissions = userPermissions & (~memberOverwrite.Deny) | memberOverwrite.Allow;
            }

            // Check for correct permissions
            return userPermissions.HasFlag(DiscordPermission.Administrator) | userPermissions.HasFlag(permission);
        }

        /// <summary>
        /// Checks whether a member has a set of permissions,
        /// if they don't a <see cref="DiscordPermissionException"/> is thrown with details.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordPermissionException"></exception>
        public static void AssertPermission(DiscordPermission permission,
            DiscordGuildMember member, DiscordGuild guild)
        {
            if (!HasPermission(permission, member, guild))
                throw new DiscordPermissionException(member, guild, permission);
        }

        /// <summary>
        /// Checks whether a member has a set of permissions in the context of a guild channel,
        /// if they don't a <see cref="DiscordPermissionException"/> is thrown with details.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordPermissionException"></exception>
        public static void AssertPermission(DiscordPermission permission,
            DiscordGuildMember member, DiscordGuild guild, DiscordGuildChannel forChannel)
        {
            if (!HasPermission(permission, member, guild, forChannel))
                throw new DiscordPermissionException(member, guild, forChannel, permission);
        }
    }
}
