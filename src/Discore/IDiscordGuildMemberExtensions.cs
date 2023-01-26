using System;

namespace Discore
{
    public static class IDiscordGuildMemberExtensions
    {
        /// <summary>
        /// Returns whether this guild member has a set of permissions within the guild and optionally
        /// additionally within a specific guild channel.
        /// </summary>
        /// <param name="permissions">The set of permissions to check if the member has.</param>
        /// <param name="guild">The guild this member is in.</param>
        /// <param name="channel">A guild channel. If null, channel permissions will not be considered.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="member"/> or <paramref name="channel"/> is not in the 
        /// specified <paramref name="guild"/>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="member"/> or <paramref name="guild"/> is null.
        /// </exception>
        public static bool HasPermission(
            this IDiscordGuildMember member,
            DiscordPermission permissions,
            DiscordGuild guild,
            DiscordGuildChannel? channel = null)
        {
            return DiscordPermissionHelper.HasPermission(permissions, member, guild, channel);
        }

        /// <summary>
        /// Returns whether this guild member can join a given voice channel.
        /// </summary>
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
            this IDiscordGuildMember member,
            DiscordGuild guild,
            DiscordGuildVoiceChannel voiceChannel,
            int? usersInVoiceChannel = null)
        {
            return DiscordPermissionHelper.CanJoinVoiceChannel(member, guild, voiceChannel, usersInVoiceChannel);
        }
    }
}
