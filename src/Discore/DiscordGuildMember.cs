using Discore.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Discore
{
    public class DiscordGuildMember : IDiscordObject, ICacheable
    {
        public string Id { get { return User?.Id; } }
        public DiscordGuild Guild { get; private set; }
        public DiscordUser User { get; private set; }
        public string Nickname { get; private set; }
        public DiscordRole[] Roles { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public DiscordVoiceState VoiceState { get; private set; }

        DiscordApiCache cache;

        public DiscordGuildMember(IDiscordClient client, DiscordGuild guild)
        {
            cache = client.Cache;

            Guild = guild;
            VoiceState = new DiscordVoiceState(client);
        }

        public bool HasPermission(DiscordPermission permission)
        {
            // Calculate permissions from member roles
            DiscordPermission userPermissions = 0;
            for (int i = 0; i < Roles.Length; i++)
                userPermissions = userPermissions | Roles[i].Permissions;

            // Check for permission
            return userPermissions.HasFlag(DiscordPermission.Administrator) || userPermissions.HasFlag(permission);
        }

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

        public void AssertPermission(DiscordPermission permission, DiscordGuild guild)
        {
            if (!HasPermission(permission))
                throw new DiscordPermissionException(this, permission);
        }

        public void AssertPermission(DiscordPermission permission, DiscordGuildChannel channel)
        {
            if (!HasPermission(permission, channel))
                throw new DiscordPermissionException(this, channel, permission);
        }

        public bool HasRole(string roleName)
        {
            for (int i = 0; i < Roles.Length; i++)
                if (Roles[i].Name == roleName)
                    return true;

            return false;
        }

        public void Update(DiscordApiData data)
        {
            Nickname = data.GetString("nick") ?? Nickname;
            JoinedAt = data.GetDateTime("joined_at") ?? JoinedAt;

            // Update roles
            IReadOnlyList<DiscordApiData> rolesData = data.GetArray("roles");
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

            // Update voice state
            VoiceState.Update(data);
        }

        public override string ToString()
        {
            return User.Username;
        }
    }
}
