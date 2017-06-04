using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets a list of all roles in a guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordRole>> GetGuildRoles(Snowflake guildId)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/roles",
                $"guilds/{guildId}/roles").ConfigureAwait(false);

            DiscordRole[] roles = new DiscordRole[data.Values.Count];
            for (int i = 0; i < roles.Length; i++)
                roles[i] = new DiscordRole(this, guildId, data.Values[i]);

            return roles;
        }

        /// <summary>
        /// Creates a new role with default settings for a guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordRole> CreateGuildRole(Snowflake guildId)
        {
            DiscordApiData data = await rest.Post($"guilds/{guildId}/roles",
                $"guilds/{guildId}/roles").ConfigureAwait(false);
            return new DiscordRole(this, guildId, data);
        }

        /// <summary>
        /// Creates a new role for a guild.
        /// </summary>
        /// <param name="parameters">A set of optional parameters to use when creating the role.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordRole> CreateGuildRole(Snowflake guildId, CreateRoleParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await rest.Post($"guilds/{guildId}/roles", requestData,
                $"guilds/{guildId}/roles").ConfigureAwait(false);
            return new DiscordRole(this, guildId, returnData);
        }

        /// <summary>
        /// Modifies the sorting positions of roles in a guild.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordRole>> ModifyGuildRolePositions(Snowflake guildId,
            IEnumerable<PositionParameters> positions)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));

            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Array);
            foreach (PositionParameters positionParam in positions)
                requestData.Values.Add(positionParam.Build());

            DiscordApiData returnData = await rest.Patch($"guilds/{guildId}/roles", requestData,
                $"guilds/{guildId}/roles").ConfigureAwait(false);

            DiscordRole[] roles = new DiscordRole[returnData.Values.Count];
            for (int i = 0; i < roles.Length; i++)
                roles[i] = new DiscordRole(this, guildId, returnData.Values[i]);

            return roles;
        }

        /// <summary>
        /// Modifies the settings of a guild role.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordRole> ModifyGuildRole(Snowflake guildId, Snowflake roleId, ModifyRoleParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await rest.Patch($"guilds/{guildId}/roles/{roleId}", requestData,
                $"guilds/{guildId}/roles/role").ConfigureAwait(false);
            return new DiscordRole(this, guildId, returnData);
        }

        /// <summary>
        /// Deletes a role from a guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteGuildRole(Snowflake guildId, Snowflake roleId)
        {
            await rest.Delete($"guilds/{guildId}/roles/{roleId}",
                $"guilds/{guildId}/roles/role").ConfigureAwait(false);
        }

        /// <summary>
        /// Adds a role to a guild member.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task AddGuildMemberRole(Snowflake guildId, Snowflake userId, Snowflake roleId)
        {
            await rest.Put($"guilds/{guildId}/members/{userId}/roles/{roleId}",
                $"guilds/{guildId}/members/member/roles/role").ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a role from a guild member.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task RemoveGuildMemberRole(Snowflake guildId, Snowflake userId, Snowflake roleId)
        {
            await rest.Delete($"guilds/{guildId}/members/{userId}/roles/{roleId}",
                $"guilds/{guildId}/members/member/roles/role").ConfigureAwait(false);
        }
    }
}
