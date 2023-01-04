using Discore.Http.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets a guild by ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuild> GetGuild(Snowflake guildId)
        {
            using JsonDocument? data = await api.Get($"guilds/{guildId}",
                $"guilds/{guildId}").ConfigureAwait(false);

            return new DiscordGuild(data!.RootElement);
        }

        /// <summary>
        /// Creates a new guild.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuild> CreateGuild(CreateGuildOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            string requestData = BuildJsonContent(options.Build);

            using JsonDocument? returnData = await api.Post("guilds", jsonContent: requestData,
                "guilds").ConfigureAwait(false);

            return new DiscordGuild(returnData!.RootElement);
        }

        /// <summary>
        /// Changes the settings of a guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuild> ModifyGuild(Snowflake guildId, ModifyGuildOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string requestData = BuildJsonContent(options.Build);

            using JsonDocument? returnData = await api.Patch($"guilds/{guildId}", jsonContent: requestData,
                $"guilds/{guildId}").ConfigureAwait(false);

            return new DiscordGuild(returnData!.RootElement);
        }

        /// <summary>
        /// Changes the settings of a guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuild> ModifyGuild(DiscordGuild guild, ModifyGuildOptions options)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return ModifyGuild(guild.Id, options);
        }

        /// <summary>
        /// Deletes a guild permanently.
        /// <para>Note: current bot must be the owner.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteGuild(Snowflake guildId)
        {
            await api.Delete($"guilds/{guildId}",
                $"guilds/{guildId}").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a guild permanently.
        /// <para>Note: current bot must be the owner.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task DeleteGuild(DiscordGuild guild)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return DeleteGuild(guild.Id);
        }

        /// <summary>
        /// Gets a list of all users that are banned from the specified guild.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordGuildBan>> GetGuildBans(Snowflake guildId)
        {
            using JsonDocument? data = await api.Get($"guilds/{guildId}/bans",
                $"guilds/{guildId}/bans").ConfigureAwait(false);

            JsonElement values = data!.RootElement;

            var users = new DiscordGuildBan[values.GetArrayLength()];
            for (int i = 0; i < users.Length; i++)
                users[i] = new DiscordGuildBan(values[i]);

            return users;
        }

        /// <summary>
        /// Gets a list of all users that are banned from the specified guild.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordGuildBan>> GetGuildBans(DiscordGuild guild)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return GetGuildBans(guild.Id);
        }

        /// <summary>
        /// Bans a user from the specified guild.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <param name="deleteMessageDays">Number of days to delete messages for (0-7) or null to delete none.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task CreateGuildBan(Snowflake guildId, Snowflake userId, int? deleteMessageDays = null)
        {
            var parameters = new UrlParametersBuilder();
            parameters["delete_message_days"] = deleteMessageDays?.ToString();

            await api.Put($"guilds/{guildId}/bans/{userId}{parameters.ToQueryString()}",
                $"guilds/{guildId}/bans/user").ConfigureAwait(false);
        }

        /// <summary>
        /// Bans a user from the specified guild.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <param name="deleteMessageDays">Number of days to delete messages for (0-7) or null to delete none.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="guild"/> or <paramref name="user"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task CreateGuildBan(DiscordGuild guild, DiscordUser user, int? deleteMessageDays = null)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));
            if (user == null) throw new ArgumentNullException(nameof(user));

            return CreateGuildBan(guild.Id, user.Id, deleteMessageDays);
        }

        /// <summary>
        /// Bans a user from the specified guild.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <param name="deleteMessageDays">Number of days to delete messages for (0-7) or null to delete none.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="member"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task CreateGuildBan(DiscordGuildMember member, int? deleteMessageDays = null)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            return CreateGuildBan(member.GuildId, member.Id, deleteMessageDays);
        }

        /// <summary>
        /// Removes a user ban from the specified guild.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task RemoveGuildBan(Snowflake guildId, Snowflake userId)
        {
            await api.Delete($"guilds/{guildId}/bans/{userId}",
                $"guilds/{guildId}/bans/user").ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a user ban from the specified guild.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task RemoveGuildBan(DiscordGuild guild, DiscordUser user)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));
            if (user == null) throw new ArgumentNullException(nameof(user));

            return RemoveGuildBan(guild, user);
        }

        /// <summary>
        /// Returns the number of members that would be kicked from a guild prune operation.
        /// <para>Requires <see cref="DiscordPermission.KickMembers"/>.</para>
        /// </summary>
        /// <param name="days">The number of days to count prune for (1-30).</param>
        /// <param name="includeRoles">
        /// By default, prune will not remove users with roles. You can optionally include specific 
        /// roles in your prune by providing the <paramref name="includeRoles"/> parameter. Any inactive 
        /// user that has a subset of the provided role(s) will be counted in the prune and users with 
        /// additional roles will not.
        /// </param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<int> GetGuildPruneCount(Snowflake guildId, int? days = null, IEnumerable<Snowflake>? includeRoles = null)
        {
            var parameters = new UrlParametersBuilder();
            if (days != null)
                parameters["days"] = days.Value.ToString();
            if (includeRoles != null)
                parameters["include_roles"] = string.Join(",", includeRoles.Select(r => r.ToString()));

            using JsonDocument? data = await api.Get($"guilds/{guildId}/prune{parameters.ToQueryString()}",
                $"guilds/{guildId}/prune").ConfigureAwait(false);

            return data!.RootElement.GetProperty("pruned").GetInt32();
        }

        /// <summary>
        /// Returns the number of members that would be kicked from a guild prune operation.
        /// <para>Requires <see cref="DiscordPermission.KickMembers"/>.</para>
        /// </summary>
        /// <param name="days">The number of days to count prune for (1-30).</param>
        /// <param name="includeRoles">
        /// By default, prune will not remove users with roles. You can optionally include specific 
        /// roles in your prune by providing the <paramref name="includeRoles"/> parameter. Any inactive 
        /// user that has a subset of the provided role(s) will be counted in the prune and users with 
        /// additional roles will not.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<int> GetGuildPruneCount(DiscordGuild guild, int? days = null, IEnumerable<DiscordRole>? includeRoles = null)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return GetGuildPruneCount(guild.Id, days, includeRoles?.Select(r => r.Id));
        }

        /// <summary>
        /// Begins a member prune operation, 
        /// kicking every member that has been offline for the specified number of days.
        /// <para>Requires <see cref="DiscordPermission.KickMembers"/>.</para>
        /// </summary>
        /// <param name="days">The number of days to prune (1-30).</param>
        /// <param name="includeRoles">
        /// By default, prune will not remove users with roles. You can optionally include specific 
        /// roles in your prune by providing the <paramref name="includeRoles"/> parameter. Any inactive 
        /// user that has a subset of the provided role(s) will be counted in the prune and users with 
        /// additional roles will not.
        /// </param>
        /// <param name="computePruneCount">
        /// For large guilds it's recommended to set this to false. When false, this method will return null.
        /// </param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<int?> BeginGuildPrune(
            Snowflake guildId, 
            int? days = null, 
            IEnumerable<Snowflake>? includeRoles = null,
            bool? computePruneCount = null)
        {
            string requestData = BuildJsonContent(writer =>
            {
                writer.WriteStartObject();
                
                if (days != null)
                    writer.WriteNumber("days", days.Value);

                if (includeRoles != null)
                {
                    writer.WriteStartArray("include_roles");

                    foreach (Snowflake roleId in includeRoles)
                        writer.WriteSnowflakeValue(roleId);

                    writer.WriteEndArray();
                }

                if (computePruneCount != null)
                    writer.WriteBoolean("compute_prune_count", computePruneCount.Value);

                writer.WriteEndObject();
            });

            using JsonDocument? data = await api.Post($"guilds/{guildId}/prune", jsonContent: requestData,
                $"guilds/{guildId}/prune").ConfigureAwait(false);

            return data!.RootElement.GetProperty("pruned").GetInt32OrNull();
        }

        /// <summary>
        /// Begins a member prune operation, 
        /// kicking every member that has been offline for the specified number of days.
        /// <para>Requires <see cref="DiscordPermission.KickMembers"/>.</para>
        /// </summary>
        /// <param name="days">The number of days to prune (1-30).</param>
        /// <param name="includeRoles">
        /// By default, prune will not remove users with roles. You can optionally include specific 
        /// roles in your prune by providing the <paramref name="includeRoles"/> parameter. Any inactive 
        /// user that has a subset of the provided role(s) will be counted in the prune and users with 
        /// additional roles will not.
        /// </param>
        /// <param name="computePruneCount">
        /// For large guilds it's recommended to set this to false. When false, this method will return null.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<int?> BeginGuildPrune(
            DiscordGuild guild,
            int? days = null,
            IEnumerable<DiscordRole>? includeRoles = null,
            bool? computePruneCount = null)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return BeginGuildPrune(guild.Id, days, includeRoles?.Select(r => r.Id), computePruneCount);
        }

        /// <summary>
        /// Gets a list of integrations for the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrations(Snowflake guildId)
        {
            using JsonDocument? data = await api.Get($"guilds/{guildId}/integrations",
                $"guilds/{guildId}/integrations").ConfigureAwait(false);

            JsonElement values = data!.RootElement;

            var integrations = new DiscordIntegration[values.GetArrayLength()];
            for (int i = 0; i < integrations.Length; i++)
                integrations[i] = new DiscordIntegration(values[i], guildId);

            return integrations;
        }

        /// <summary>
        /// Gets a list of integrations for the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrations(DiscordGuild guild)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return GetGuildIntegrations(guild.Id);
        }

        /// <summary>
        /// Attaches an integration from the current bot to the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task CreateGuildIntegration(Snowflake guildId, Snowflake integrationId, string type)
        {
            string requestData = BuildJsonContent(writer =>
            {
                writer.WriteStartObject();
                writer.WriteSnowflake("id", integrationId);
                writer.WriteString("type", type);
                writer.WriteEndObject();
            });

            await api.Post($"guilds/{guildId}/integrations", jsonContent: requestData,
                $"guilds/{guildId}/integrations").ConfigureAwait(false);
        }

        /// <summary>
        /// Attaches an integration from the current bot to the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task CreateGuildIntegration(DiscordGuild guild, DiscordIntegration integration)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));
            if (integration == null) throw new ArgumentNullException(nameof(integration));

            return CreateGuildIntegration(guild.Id, integration.Id, integration.Type);
        }

        /// <summary>
        /// Changes the attributes of a guild integration.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task ModifyGuildIntegration(Snowflake guildId, Snowflake integrationId,
            ModifyIntegrationOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string requestData = BuildJsonContent(options.Build);

            await api.Patch($"guilds/{guildId}/integrations/{integrationId}", jsonContent: requestData,
                $"guilds/{guildId}/integrations/integration").ConfigureAwait(false);
        }

        /// <summary>
        /// Changes the attributes of a guild integration.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="integration"/>.GuildId is null.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task ModifyGuildIntegration(DiscordIntegration integration,
            ModifyIntegrationOptions options)
        {
            if (integration == null) throw new ArgumentNullException(nameof(integration));

            if (integration.GuildId == null)
                throw new ArgumentException("The given integration is not a guild integration.");

            return ModifyGuildIntegration(integration.GuildId.Value, integration.Id, options);
        }

        /// <summary>
        /// Deletes an attached integration from the specified channel.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteGuildIntegration(Snowflake guildId, Snowflake integrationId)
        {
            await api.Delete($"guilds/{guildId}/integrations/{integrationId}",
                $"guilds/{guildId}/integrations/integration").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an attached integration from the specified channel.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="integration"/>.GuildId is null.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task DeleteGuildIntegration(DiscordIntegration integration)
        {
            if (integration == null) throw new ArgumentNullException(nameof(integration));

            if (integration.GuildId == null)
                throw new ArgumentException("The given integration is not a guild integration.");

            return DeleteGuildIntegration(integration.GuildId.Value, integration.Id);
        }

        /// <summary>
        /// Synchronizes a guild integration.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task SyncGuildIntegration(Snowflake guildId, Snowflake integrationId)
        {
            await api.Post($"guilds/{guildId}/integrations/{integrationId}/sync",
                $"guilds/{guildId}/integrations/integration/sync").ConfigureAwait(false);
        }

        /// <summary>
        /// Synchronizes a guild integration.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <paramref name="integration"/>.GuildId is null.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task SyncGuildIntegration(DiscordIntegration integration)
        {
            if (integration == null) throw new ArgumentNullException(nameof(integration));

            if (integration.GuildId == null)
                throw new ArgumentException("The given integration is not a guild integration.");

            return SyncGuildIntegration(integration.GuildId.Value, integration.Id);
        }

        /// <summary>
        /// Returns the embed for the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildWidgetSettings> GetGuildWidget(Snowflake guildId)
        {
            using JsonDocument? data = await api.Get($"guilds/{guildId}/widget",
                $"guilds/{guildId}/widget").ConfigureAwait(false);

            return new DiscordGuildWidgetSettings(data!.RootElement, guildId: guildId);
        }

        /// <summary>
        /// Returns the embed for the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildWidgetSettings> GetGuildWidget(DiscordGuild guild)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return GetGuildWidget(guild.Id);
        }

        /// <summary>
        /// Modifies the properties of the widget for the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildWidgetSettings> ModifyGuildWidget(Snowflake guildId, ModifyGuildWidgetOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string requestData = BuildJsonContent(options.Build);

            using JsonDocument? returnData = await api.Patch($"guilds/{guildId}/widget", jsonContent: requestData,
                $"guilds/{guildId}/widget").ConfigureAwait(false);

            return new DiscordGuildWidgetSettings(returnData!.RootElement, guildId: guildId);
        }

        /// <summary>
        /// Modifies the properties of the widget for the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildWidgetSettings> ModifyGuildWidget(DiscordGuild guild, ModifyGuildWidgetOptions options)
        {
            if (guild == null) throw new ArgumentNullException(nameof(guild));

            return ModifyGuildWidget(guild.Id, options);
        }
    }
}
