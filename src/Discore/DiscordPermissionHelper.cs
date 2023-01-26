using System;
using System.Collections.Generic;
using System.Text;

// Permission logic here is based off of:
// https://discord.com/developers/docs/topics/permissions#permission-hierarchy

namespace Discore
{
    /// <summary>
    /// Utilities for working with Discord user permissions.
    /// </summary>
    public static class DiscordPermissionHelper
    {
        /// <summary>
        /// Returns whether a guild member has a set of permissions within the guild and optionally
        /// additionally within a specific guild channel.
        /// </summary>
        /// <param name="permissions">The set of permissions to check if the member has.</param>
        /// <param name="memberId">The ID of the guild member to check.</param>
        /// <param name="memberRoleIds">A list of role IDs that the member has.</param>
        /// <param name="guildId">The ID of the guild.</param>
        /// <param name="guildOwnerId">The ID of the owner of the guild.</param>
        /// <param name="guildRoles">A map of the guild's roles.</param>
        /// <param name="channelOverwrites">
        /// A map of the channel's roles. If null, channel permissions will not be considered.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="memberRoleIds"/> or <paramref name="guildRoles"/> is null.
        /// </exception>
        public static bool HasPermission(
            DiscordPermission permissions,
            Snowflake memberId,
            IEnumerable<Snowflake> memberRoleIds,
            Snowflake guildId,
            Snowflake guildOwnerId,
            IReadOnlyDictionary<Snowflake, DiscordRole> guildRoles,
            IReadOnlyDictionary<Snowflake, DiscordOverwrite>? channelOverwrites = null)
        {
            if (memberRoleIds == null)
                throw new ArgumentNullException(nameof(memberRoleIds));
            if (guildRoles == null)
                throw new ArgumentNullException(nameof(guildRoles));

            // If owner, everything is true
            if (memberId == guildOwnerId)
                return true;

            // Apply @everyone permissions
            DiscordRole everyoneRole = guildRoles[guildId];

            DiscordPermission userPermissions = everyoneRole.Permissions;

            // Apply permissions for each role the member has
            foreach (Snowflake roleId in memberRoleIds)
            {
                if (guildRoles.TryGetValue(roleId, out DiscordRole? role))
                {
                    userPermissions = userPermissions | role.Permissions;
                }
            }

            if (channelOverwrites != null)
            {
                // Administrator overrides channel-specific overwrites
                if (userPermissions.HasFlag(DiscordPermission.Administrator))
                    return true;

                // Apply channel @everyone overwrites
                DiscordOverwrite? channelEveryoneOverwrite;
                if (channelOverwrites.TryGetValue(guildId, out channelEveryoneOverwrite))
                {
                    userPermissions = (userPermissions & (~channelEveryoneOverwrite.Deny)) | channelEveryoneOverwrite.Allow;
                }

                // Apply channel-specific role overwrites
                DiscordPermission roleOverwriteAllow = 0;
                DiscordPermission roleOverwriteDeny = 0;

                foreach (Snowflake roleId in memberRoleIds)
                {
                    DiscordOverwrite? overwrite;
                    if (channelOverwrites.TryGetValue(roleId, out overwrite))
                    {
                        roleOverwriteAllow = roleOverwriteAllow | overwrite.Allow;
                        roleOverwriteDeny = roleOverwriteDeny | overwrite.Deny;
                    }
                }

                userPermissions = (userPermissions & (~roleOverwriteDeny)) | roleOverwriteAllow;

                // Apply channel-specific member overwrite for this channel
                DiscordOverwrite? memberOverwrite;
                if (channelOverwrites.TryGetValue(memberId, out memberOverwrite))
                {
                    userPermissions = (userPermissions & (~memberOverwrite.Deny)) | memberOverwrite.Allow;
                }
            }

            // Check for permission
            return userPermissions.HasFlag(DiscordPermission.Administrator) || userPermissions.HasFlag(permissions);
        }

        /// <summary>
        /// Returns whether a guild member has a set of permissions within the guild and optionally
        /// additionally within a specific guild channel.
        /// </summary>
        /// <param name="permissions">The set of permissions to check if the member has.</param>
        /// <param name="member">The guild member to check.</param>
        /// <param name="guild">The guild.</param>
        /// <param name="channel">A guild channel. If null, channel permissions will not be considered.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="member"/> or <paramref name="channel"/> is not in the 
        /// specified <paramref name="guild"/>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="member"/> or <paramref name="guild"/> is null.
        /// </exception>
        public static bool HasPermission(
            DiscordPermission permissions,
            IDiscordGuildMember member,
            DiscordGuild guild,
            DiscordGuildChannel? channel = null)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            if (guild == null)
                throw new ArgumentNullException(nameof(guild));

            if (member.GuildId != guild.Id)
                throw new ArgumentException("Member must be in the specified guild.");
            if (channel != null && channel.GuildId != member.GuildId)
                throw new ArgumentException("Guild channel must be in the same guild as this member.");
            if (channel != null && channel.GuildId != guild.Id)
                throw new ArgumentException("Guild channel must be in the specified guild.");

            return HasPermission(
                permissions,
                memberId: member.Id,
                memberRoleIds: member.RoleIds,
                guildId: guild.Id,
                guildOwnerId: guild.OwnerId,
                guildRoles: guild.Roles,
                channelOverwrites: channel?.PermissionOverwrites);
        }

        /// <summary>
        /// Returns whether a guild member can join a given voice channel.
        /// </summary>
        /// <param name="member">The guild member to check.</param>
        /// <param name="guild">The guild the member is in.</param>
        /// <param name="voiceChannel">The voice channel to check if the member can join.</param>
        /// <param name="usersInVoiceChannel">
        /// The number of users currently in the <paramref name="voiceChannel"/>. If null, this will
        /// not be checked and it will be assumed that there is room in the channel.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="member"/> or <paramref name="voiceChannel"/> is not in the 
        /// specified <paramref name="guild"/>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="member"/>, <paramref name="guild"/>, or <paramref name="voiceChannel"/> is null.
        /// </exception>
        public static bool CanJoinVoiceChannel(
            IDiscordGuildMember member,
            DiscordGuild guild,
            DiscordGuildVoiceChannel voiceChannel,
            int? usersInVoiceChannel = null)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));
            if (voiceChannel == null)
                throw new ArgumentNullException(nameof(voiceChannel));
            if (guild == null)
                throw new ArgumentNullException(nameof(guild));

            if (voiceChannel.GuildId != member.GuildId)
                throw new ArgumentException("Voice channel must be in the same guild as this member.");
            if (voiceChannel.GuildId != guild.Id)
                throw new ArgumentException("Voice channel must be in the specified guild.");

            // Check if the user has permission to connect.
            if (!HasPermission(DiscordPermission.Connect, member, guild, voiceChannel))
                return false;

            if (usersInVoiceChannel == null)
            {
                // Assume there's room
                return true;
            }
            else
            {
                // Check if the voice channel has room
                bool channelHasRoom;
                if (voiceChannel.UserLimit == 0)
                    channelHasRoom = true;
                else if (HasPermission(DiscordPermission.Administrator, member, guild, voiceChannel))
                    channelHasRoom = true;
                else
                    channelHasRoom = usersInVoiceChannel.Value < voiceChannel.UserLimit;

                return channelHasRoom;
            }
        }

        /// <summary>
        /// Creates a comma-delimited string of permission names specified by the given bitwise permission flag.
        /// </summary>
        /// <param name="permission">The bitwise permission flag to convert.</param>
        /// <returns>A comma-delimited string in the format: "Name, Name2, Name3" (without quotes).</returns>
        public static string PermissionsToString(DiscordPermission permission)
        {
            if (permission == DiscordPermission.None)
                return DiscordPermission.None.ToString();

            var sb = new StringBuilder();
            foreach (DiscordPermission value in Enum.GetValues<DiscordPermission>())
            {
                if (value != DiscordPermission.None && permission.HasFlag(value))
                {
                    if (sb.Length == 0)
                    {
                        sb.Append(value);
                    }
                    else
                    {
                        sb.Append(", ");
                        sb.Append(value);
                    }
                }
            }

            return sb.ToString();
        }
    }
}
