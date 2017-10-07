using Discore.Http.Net;
using System;
using System.Collections.Generic;
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
            DiscordApiData data = await rest.Get($"guilds/{guildId}",
                $"guilds/{guildId}").ConfigureAwait(false);
            return new DiscordGuild(this, data);
        }

        /// <summary>
        /// Creates a new guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuild> CreateGuild(CreateGuildOptions options)
        {
            DiscordApiData requestdata = options.Build();

            DiscordApiData returnData = await rest.Post("guilds", requestdata,
                "guilds").ConfigureAwait(false);

            return new DiscordGuild(this, returnData);
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

            DiscordApiData requestData = options.Build();

            DiscordApiData returnData = await rest.Patch($"guilds/{guildId}", requestData,
                $"guilds/{guildId}").ConfigureAwait(false);
            return new DiscordGuild(this, returnData);
        }

        /// <summary>
        /// Deletes a guild permanently.
        /// <para>Note: current bot must be the owner.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteGuild(Snowflake guildId)
        {
            await rest.Delete($"guilds/{guildId}",
                $"guilds/{guildId}").ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a list of all users that are banned from the specified guild.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordGuildBan>> GetGuildBans(Snowflake guildId)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/bans",
                $"guilds/{guildId}/bans").ConfigureAwait(false);

            DiscordGuildBan[] users = new DiscordGuildBan[data.Values.Count];
            for (int i = 0; i < users.Length; i++)
                users[i] = new DiscordGuildBan(data.Values[i]);

            return users;
        }

        /// <summary>
        /// Bans a users from the specified guild.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <param name="deleteMessageDays">Number of days to delete messages for (0-7) or null to delete none.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task CreateGuildBan(Snowflake guildId, Snowflake userId, int? deleteMessageDays = null)
        {
            UrlParametersBuilder parameters = new UrlParametersBuilder();
            parameters["delete-message-days"] = deleteMessageDays?.ToString();

            await rest.Put($"guilds/{guildId}/bans/{userId}{parameters.ToQueryString()}",
                $"guilds/{guildId}/bans/user").ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a user ban from the specified guild.
        /// <para>Requires <see cref="DiscordPermission.BanMembers"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task RemoveGuildBan(Snowflake guildId, Snowflake userId)
        {
            await rest.Delete($"guilds/{guildId}/bans/{userId}",
                $"guilds/{guildId}/bans/user").ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the number of members that would be kicked from a guild prune operation.
        /// <para>Requires <see cref="DiscordPermission.KickMembers"/>.</para>
        /// </summary>
        /// <param name="days">The number of days to count prune for (1 or more).</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<int> GetGuildPruneCount(Snowflake guildId, int days)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/prune?days={days}",
                $"guilds/{guildId}/prune").ConfigureAwait(false);
            return data.GetInteger("pruned").Value;
        }

        /// <summary>
        /// Begins a member prune operation, 
        /// kicking every member that has been offline for the specified number of days.
        /// <para>Requires <see cref="DiscordPermission.KickMembers"/>.</para>
        /// </summary>
        /// <param name="days">The number of days to prune (1 or more).</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<int> BeginGuildPrune(Snowflake guildId, int days)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("days", days);

            DiscordApiData data = await rest.Post($"guilds/{guildId}/prune", requestData,
                $"guilds/{guildId}/prune").ConfigureAwait(false);
            return data.GetInteger("pruned").Value;
        }

        /// <summary>
        /// Gets a list of integrations for the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrations(Snowflake guildId)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/integrations",
                $"guilds/{guildId}/integrations").ConfigureAwait(false);

            DiscordIntegration[] integrations = new DiscordIntegration[data.Values.Count];
            for (int i = 0; i < integrations.Length; i++)
                integrations[i] = new DiscordIntegration(this, data.Values[i], guildId);

            return integrations;
        }

        /// <summary>
        /// Attaches an integration from the current bot to the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task CreateGuildIntegration(Snowflake guildId, Snowflake integrationId, string type)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("id", integrationId);
            requestData.Set("type", type);

            await rest.Post($"guilds/{guildId}/integrations",
                $"guilds/{guildId}/integrations").ConfigureAwait(false);
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

            DiscordApiData requestData = options.Build();

            await rest.Patch($"guilds/{guildId}/integrations/{integrationId}", requestData,
                $"guilds/{guildId}/integrations/integration").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an attached integration from the specified channel.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteGuildIntegration(Snowflake guildId, Snowflake integrationId)
        {
            await rest.Delete($"guilds/{guildId}/integrations/{integrationId}",
                $"guilds/{guildId}/integrations/integration").ConfigureAwait(false);
        }

        /// <summary>
        /// Synchronizes a guild integration.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task SyncGuildIntegration(Snowflake guildId, Snowflake integrationId)
        {
            await rest.Post($"guilds/{guildId}/integrations/{integrationId}/sync",
                $"guilds/{guildId}/integrations/integration/sync").ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the embed for the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildEmbed> GetGuildEmbed(Snowflake guildId)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/embed",
                $"guilds/{guildId}/embed").ConfigureAwait(false);
            return new DiscordGuildEmbed(this, guildId, data);
        }

        /// <summary>
        /// Modifies the properties of the embed for the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildEmbed> ModifyGuildEmbed(Snowflake guildId, ModifyGuildEmbedOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            DiscordApiData requestData = options.Build();

            DiscordApiData returnData = await rest.Patch($"guilds/{guildId}/embed", requestData,
                $"guilds/{guildId}/embed").ConfigureAwait(false);
            return new DiscordGuildEmbed(this, guildId, returnData);
        }
    }
}
