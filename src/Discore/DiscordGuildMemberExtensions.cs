using System;

#nullable enable

namespace Discore
{
    public static class DiscordGuildMemberExtensions
    {
        /// <summary>
        /// Returns whether this member has the given set of permissions.
        /// </summary>
        /// <param name="permissions">The set of permissions to check if the member has.</param>
        /// <param name="guild">The guild this member is in.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="member"/> is not in the specified <paramref name="guild"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="member"/> or <paramref name="guild"/> is null.
        /// </exception>
        public static bool HasPermission(this DiscordGuildMember member, DiscordPermission permissions, DiscordGuild guild)
            => DiscordPermissionHelper.HasPermission(permissions, member, guild);

        /// <summary>
        /// Returns whether this member has the given set of permissions
        /// in the context of the specified guild channel.
        /// </summary>
        /// <param name="permissions">The set of permissions to check if the member has.</param>
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
        public static bool HasPermission(this DiscordGuildMember member, DiscordPermission permissions, DiscordGuild guild, DiscordGuildChannel channel)
            => DiscordPermissionHelper.HasPermission(permissions, member, guild, channel);

        /// <summary>
        /// Checks whether this member has the given set of permissions.
        /// If they don't, a <see cref="DiscordPermissionException"/> is thrown with details.
        /// </summary>
        /// <param name="permissions">The set of permissions to check if the member has.</param>
        /// <param name="guild">The guild this member is in.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="member"/> is not in the specified <paramref name="guild"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="member"/> or <paramref name="guild"/> is null.
        /// </exception>
        public static void AssertPermission(this DiscordGuildMember member, DiscordPermission permissions, DiscordGuild guild)
            => DiscordPermissionHelper.AssertPermission(permissions, member, guild);

        /// <summary>
        /// Checks whether this member has the given set of permissions 
        /// in the context of the specified guild channel.
        /// If they don't, a <see cref="DiscordPermissionException"/> is thrown with details.
        /// </summary>
        /// <param name="permissions">The set of permissions to check if the member has.</param>
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
        public static void AssertPermission(this DiscordGuildMember member, DiscordPermission permissions, DiscordGuild guild, DiscordGuildChannel channel)
            => DiscordPermissionHelper.AssertPermission(permissions, member, guild, channel);
    }
}

#nullable restore
