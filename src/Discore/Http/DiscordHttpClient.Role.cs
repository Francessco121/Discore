using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets a list of all roles in a guild.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordRole>> GetGuildRoles(Snowflake guildId)
        {
            using JsonDocument? data = await api.Get($"guilds/{guildId}/roles",
                $"guilds/{guildId}/roles").ConfigureAwait(false);

            JsonElement values = data!.RootElement;

            var roles = new DiscordRole[values.GetArrayLength()];
            for (int i = 0; i < roles.Length; i++)
                roles[i] = new DiscordRole(values[i], guildId: guildId);

            return roles;
        }

        /// <summary>
        /// Gets a list of all roles in a guild.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordRole>> GetGuildRoles(DiscordGuild guild)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return GetGuildRoles(guild.Id);
        }

        /// <summary>
        /// Creates a new role for a guild.
        /// </summary>
        /// <param name="options">A set of optional options to use when creating the role.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordRole> CreateGuildRole(Snowflake guildId, CreateRoleOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string requestData = BuildJsonContent(options.Build);

            using JsonDocument? returnData = await api.Post($"guilds/{guildId}/roles", jsonContent: requestData,
                $"guilds/{guildId}/roles").ConfigureAwait(false);

            return new DiscordRole(returnData!.RootElement, guildId: guildId);
        }

        /// <summary>
        /// Creates a new role for a guild.
        /// </summary>
        /// <param name="options">A set of optional options to use when creating the role.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordRole> CreateGuildRole(DiscordGuild guild, CreateRoleOptions options)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return CreateGuildRole(guild.Id, options);
        }

        /// <summary>
        /// Modifies the sorting positions of roles in a guild.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordRole>> ModifyGuildRolePositions(Snowflake guildId,
            IEnumerable<PositionOptions> positions)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));

            string requestData = BuildJsonContent(writer =>
            {
                writer.WriteStartArray();

                foreach (PositionOptions positionParam in positions)
                    positionParam.Build(writer);

                writer.WriteEndArray();
            });

            using JsonDocument? returnData = await api.Patch($"guilds/{guildId}/roles", jsonContent: requestData,
                $"guilds/{guildId}/roles").ConfigureAwait(false);

            JsonElement values = returnData!.RootElement;

            var roles = new DiscordRole[values.GetArrayLength()];
            for (int i = 0; i < roles.Length; i++)
                roles[i] = new DiscordRole(values[i], guildId: guildId);

            return roles;
        }

        /// <summary>
        /// Modifies the sorting positions of roles in a guild.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordRole>> ModifyGuildRolePositions(DiscordGuild guild,
            IEnumerable<PositionOptions> positions)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return ModifyGuildRolePositions(guild.Id, positions);
        }

        /// <summary>
        /// Modifies the settings of a guild role.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordRole> ModifyGuildRole(Snowflake guildId, Snowflake roleId, ModifyRoleOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string requestData = BuildJsonContent(options.Build);

            using JsonDocument? returnData = await api.Patch($"guilds/{guildId}/roles/{roleId}", jsonContent: requestData,
                $"guilds/{guildId}/roles/role").ConfigureAwait(false);

            return new DiscordRole(returnData!.RootElement, guildId: guildId);
        }

        /// <summary>
        /// Modifies the settings of a guild role.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordRole> ModifyGuildRole(DiscordRole role, ModifyRoleOptions options)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            return ModifyGuildRole(role.GuildId, role.Id, options);
        }

        /// <summary>
        /// Deletes a role from a guild.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteGuildRole(Snowflake guildId, Snowflake roleId)
        {
            await api.Delete($"guilds/{guildId}/roles/{roleId}",
                $"guilds/{guildId}/roles/role").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a role from a guild.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task DeleteGuildRole(DiscordRole role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));

            return DeleteGuildRole(role.GuildId, role.Id);
        }

        /// <summary>
        /// Adds a role to a guild member.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task AddGuildMemberRole(Snowflake guildId, Snowflake userId, Snowflake roleId)
        {
            await api.Put($"guilds/{guildId}/members/{userId}/roles/{roleId}",
                $"guilds/{guildId}/members/member/roles/role").ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a role to a guild member.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task AddGuildMemberRole(DiscordGuildMember member, DiscordRole role)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (role == null) throw new ArgumentNullException(nameof(role));

            return AddGuildMemberRole(member.GuildId, member.Id, role.Id);
        }

        /// <summary>
        /// Removes a role from a guild member.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task RemoveGuildMemberRole(Snowflake guildId, Snowflake userId, Snowflake roleId)
        {
            await api.Delete($"guilds/{guildId}/members/{userId}/roles/{roleId}",
                $"guilds/{guildId}/members/member/roles/role").ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a role from a guild member.
        /// <para>Requires <see cref="DiscordPermission.ManageRoles"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task RemoveGuildMemberRole(DiscordGuildMember member, DiscordRole role)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (role == null) throw new ArgumentNullException(nameof(role));

            return RemoveGuildMemberRole(member.GuildId, member.Id, role.Id);
        }
    }
}
