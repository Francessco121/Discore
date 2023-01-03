using System;
using System.Text;

namespace Discore
{
    /// <summary>
    /// An exception thrown because of a Discord permission issue.
    /// </summary>
    public class DiscordPermissionException : Exception
    {
        /// <summary>
        /// The required permissions that caused the exception.
        /// </summary>
        public DiscordPermission Permission { get; }
        /// <summary>
        /// The member that didn't have the right permissions.
        /// </summary>
        public DiscordGuildMember Member { get; }
        /// <summary>
        /// The guild the member is in.
        /// </summary>
        public DiscordGuild Guild { get; }
        /// <summary>
        /// The channel (if applicable) the permissions were checked against.
        /// </summary>
        public DiscordGuildChannel? Channel { get; }

        /// <summary>
        /// Creates a new <see cref="DiscordPermissionException"/> instance.
        /// </summary>
        /// <param name="member">The member that didn't have the right permissions.</param>
        /// <param name="guild">The guild the member is in.</param>
        /// <param name="permission">The permissions that caused the exception.</param>
        public DiscordPermissionException(DiscordGuildMember member, DiscordGuild guild, DiscordPermission permission)
            : base($"\"{member.User.Username}\" does not have permissions: {GetPermissions(permission)}")
        {
            Permission = permission;
            Guild = guild;
            Member = member;
        }

        /// <summary>
        /// Creates a new <see cref="DiscordPermissionException"/> instance.
        /// </summary>
        /// <param name="member">The member that didn't have the right permissions.</param>
        /// <param name="guild">The guild the member is in.</param>
        /// <param name="channel">The channel the permissions were checked against.</param>
        /// <param name="permission">The permissions that caused the exception.</param>
        public DiscordPermissionException(DiscordGuildMember member, DiscordGuild guild, DiscordGuildChannel channel, DiscordPermission permission)
            : base($"\"{member.User.Username}\" does not have permissions: {GetPermissions(permission)} in channel \"{channel.Name}\"")
        {
            Permission = permission;
            Member = member;
            Guild = guild;
            Channel = channel;
        }

        static string GetPermissions(DiscordPermission permission)
        {
            var sb = new StringBuilder();
            foreach (Enum value in Enum.GetValues(typeof(DiscordPermission)))
                if (permission.HasFlag(value))
                {
                    if (sb.Length == 0)
                        sb.Append(value.ToString());
                    else
                        sb.Append($", {value}");
                }

            return sb.ToString();
        }
    }
}
