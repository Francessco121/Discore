using System;
using System.Text;

namespace Discore
{
    public class DiscordPermissionException : DiscordioException
    {
        public DiscordPermission Permission { get; }
        public DiscordGuildMember Member { get; }
        public DiscordGuildChannel Channel { get; }

        public DiscordPermissionException(DiscordGuildMember member, DiscordPermission permission)
            : base($"\"{member.User.Username}\" does not have permissions: {GetPermissions(permission)}")
        {
            Permission = permission;
            Member = member;
        }

        public DiscordPermissionException(DiscordGuildMember member, DiscordGuildChannel channel, DiscordPermission permission)
            : base($"\"{member.User.Username}\" does not have permissions: {GetPermissions(permission)} in channel \"{channel.Name}\"")
        {
            Permission = permission;
            Member = member;
            Channel = channel;
        }

        static string GetPermissions(DiscordPermission permission)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Enum value in Enum.GetValues(typeof(DiscordPermission)))
                if (permission.HasFlag(value))
                {
                    if (sb.Length == 0)
                        sb.Append(value.ToString());
                    else
                        sb.Append($", {value.ToString()}");
                }

            return sb.ToString();
        }
    }
}
