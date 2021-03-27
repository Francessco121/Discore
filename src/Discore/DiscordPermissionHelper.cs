using System;

// Permission logic here is based off of:
// https://discord.com/developers/docs/topics/permissions#permission-hierarchy

namespace Discore
{
    public static class DiscordPermissionHelper
    {
        /// <summary>
        /// Returns whether the specified member has the given set of permissions.
        /// </summary>
        /// <param name="permissions">The set of permissions to check if the member has.</param>
        /// <param name="member">The member to check the permissions of.</param>
        /// <param name="guild">The guild this member is in.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="member"/> is not in the specified <paramref name="guild"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="member"/> or <paramref name="guild"/> is null.
        /// </exception>
        public static bool HasPermission(DiscordPermission permissions, DiscordGuildMember member, DiscordGuild guild)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            if (guild == null)
                throw new ArgumentNullException(nameof(guild));

            if (member.GuildId != guild.Id)
                throw new ArgumentException("Member must be in the specified guild.");

            // If owner, everything is true
            if (member.Id == guild.OwnerId)
                return true;

            // Apply @everyone permissions
            DiscordRole everyoneRole = guild.Roles[guild.Id];

            DiscordPermission userPermissions = everyoneRole.Permissions;

            // Apply permissions for each role the member has
            foreach (Snowflake roleId in member.RoleIds)
            {
                if (guild.Roles.TryGetValue(roleId, out DiscordRole role))
                {
                    userPermissions = userPermissions | role.Permissions;
                }
            }

            // Check for permission
            return userPermissions.HasFlag(DiscordPermission.Administrator) || userPermissions.HasFlag(permissions);
        }

        /// <summary>
        /// Returns whether the specified member has the given set of permissions
        /// in the context of the specified guild channel.
        /// </summary>
        /// <param name="permissions">The set of permissions to check if the member has.</param>
        /// <param name="member">The member to check the permissions of.</param>
        /// <param name="guild">The guild this member is in.</param>
        /// <param name="channel">
        /// The guild channel to check the permissions against (this will take overwrites into account).
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="member"/> or <paramref name="channel"/> is not in the 
        /// specified <paramref name="guild"/>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="member"/>, <paramref name="guild"/>, or <paramref name="channel"/>
        /// is null.
        /// </exception>
        public static bool HasPermission(DiscordPermission permissions, 
            DiscordGuildMember member, DiscordGuild guild, DiscordGuildChannel channel)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            if (guild == null)
                throw new ArgumentNullException(nameof(guild));
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            if (channel.GuildId != member.GuildId)
                throw new ArgumentException("Guild channel must be in the same guild as this member.");
            if (channel.GuildId != guild.Id)
                throw new ArgumentException("Guild channel must be in the specified guild.");

            // If owner, everything is true
            if (member.Id == guild.OwnerId)
                return true;

            // Apply @everyone permissions
            DiscordRole everyoneRole = guild.Roles[guild.Id];

            DiscordPermission userPermissions = everyoneRole.Permissions;

            // Apply permissions for each role the member has
            foreach (Snowflake roleId in member.RoleIds)
            {
                if (guild.Roles.TryGetValue(roleId, out DiscordRole role))
                {
                    userPermissions = userPermissions | role.Permissions;
                }
            }

            // Administrator overrides channel-specific overwrites
            if (userPermissions.HasFlag(DiscordPermission.Administrator))
                return true;

            // Apply channel @everyone overwrites
            DiscordOverwrite channelEveryoneOverwrite;
            if (channel.PermissionOverwrites.TryGetValue(guild.Id, out channelEveryoneOverwrite))
            {
                userPermissions = (userPermissions & (~channelEveryoneOverwrite.Deny)) | channelEveryoneOverwrite.Allow;
            }

            // Apply channel-specific role overwrites
            DiscordPermission roleOverwriteAllow = 0;
            DiscordPermission roleOverwriteDeny = 0;

            foreach (Snowflake roleId in member.RoleIds)
            {
                DiscordOverwrite overwrite;
                if (channel.PermissionOverwrites.TryGetValue(roleId, out overwrite))
                {
                    roleOverwriteAllow = roleOverwriteAllow | overwrite.Allow;
                    roleOverwriteDeny = roleOverwriteDeny | overwrite.Deny;
                }
            }

            userPermissions = (userPermissions & (~roleOverwriteDeny)) | roleOverwriteAllow;

            // Apply channel-specific member overwrite for this channel
            DiscordOverwrite memberOverwrite;
            if (channel.PermissionOverwrites.TryGetValue(member.Id, out memberOverwrite))
            {
                userPermissions = (userPermissions & (~memberOverwrite.Deny)) | memberOverwrite.Allow;
            }

            // Check for correct permissions
            return userPermissions.HasFlag(DiscordPermission.Administrator) | userPermissions.HasFlag(permissions);
        }

        /// <summary>
        /// Checks whether the specified member has the given set of permissions.
        /// If they don't, a <see cref="DiscordPermissionException"/> is thrown with details.
        /// </summary>
        /// <param name="permissions">The set of permissions to check if the member has.</param>
        /// <param name="member">The member to check the permissions of.</param>
        /// <param name="guild">The guild this member is in.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="member"/> is not in the specified <paramref name="guild"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="member"/> or <paramref name="guild"/> is null.
        /// </exception>
        public static void AssertPermission(DiscordPermission permissions,
            DiscordGuildMember member, DiscordGuild guild)
        {
            if (!HasPermission(permissions, member, guild))
                throw new DiscordPermissionException(member, guild, permissions);
        }

        /// <summary>
        /// Checks whether the specified member has the given set of permissions 
        /// in the context of the specified guild channel.
        /// If they don't, a <see cref="DiscordPermissionException"/> is thrown with details.
        /// </summary>
        /// <param name="permissions">The set of permissions to check if the member has.</param>
        /// <param name="member">The member to check the permissions of.</param>
        /// <param name="guild">The guild this member is in.</param>
        /// <param name="channel">
        /// The guild channel to check the permissions against (this will take overwrites into account).
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="member"/> or <paramref name="channel"/> is not in the 
        /// specified <paramref name="guild"/>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="member"/>, <paramref name="guild"/>, or <paramref name="channel"/>
        /// is null.
        /// </exception>
        public static void AssertPermission(DiscordPermission permissions,
            DiscordGuildMember member, DiscordGuild guild, DiscordGuildChannel channel)
        {
            if (!HasPermission(permissions, member, guild, channel))
                throw new DiscordPermissionException(member, guild, channel, permissions);
        }
    }
}
