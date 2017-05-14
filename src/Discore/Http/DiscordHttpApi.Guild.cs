using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpApi
    {
        /// <summary>
        /// Gets a guild by ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuild> GetGuild(Snowflake guildId)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}",
                "guilds/guild").ConfigureAwait(false);
            return new DiscordGuild(app, data);
        }

        /// <summary>
        /// Creates a new guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuild> CreateGuild(CreateGuildParameters parameters)
        {
            DiscordApiData requestdata = parameters.Build();

            DiscordApiData returnData = await rest.Post("guilds", requestdata,
                "guilds").ConfigureAwait(false);

            return new DiscordGuild(app, returnData);
        }

        /// <summary>
        /// Changes the settings of a guild.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuild> ModifyGuild(Snowflake guildId, ModifyGuildParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await rest.Patch($"guilds/{guildId}", requestData,
                "guilds/guild").ConfigureAwait(false);
            return new DiscordGuild(app, returnData);
        }

        /// <summary>
        /// Deletes a guild permanently.
        /// Authenticated user must be the owner.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteGuild(Snowflake guildId)
        {
            await rest.Delete($"guilds/{guildId}",
                "guilds/guild").ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a list of all users that are banned from the specified guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordUser>> GetGuildBans(Snowflake guildId)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/bans",
                "guilds/guild/bans").ConfigureAwait(false);

            DiscordUser[] users = new DiscordUser[data.Values.Count];
            for (int i = 0; i < users.Length; i++)
                users[i] = new DiscordUser(data.Values[i]);

            return users;
        }

        /// <summary>
        /// Bans a users from the specified guild.
        /// </summary>
        /// <param name="deleteMessageDays">Number of days to delete messages for (0-7).</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task CreateGuildBan(Snowflake guildId, Snowflake userId, int? deleteMessageDays = null)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("delete-message-days", deleteMessageDays);

            await rest.Put($"guilds/{guildId}/bans/{userId}", requestData,
                "guilds/guild/bans/user").ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a user ban from the specified guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task RemoveGuildBan(Snowflake guildId, Snowflake userId)
        {
            await rest.Delete($"guilds/{guildId}/bans/{userId}",
                "guilds/guild/bans/user").ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the number of members that would be kicked from a guild prune operation.
        /// </summary>
        /// <param name="days">The number of days to count prune for (1 or more).</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<int> GetGuildPruneCount(Snowflake guildId, int days)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/prune?days={days}",
                "guilds/guild/prune").ConfigureAwait(false);
            return data.GetInteger("pruned").Value;
        }

        /// <summary>
        /// Begins a member prune operation, 
        /// kicking every member that has been offline for the specified number of days.
        /// </summary>
        /// <param name="days">The number of days to prune (1 or more).</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<int> BeginGuildPrune(Snowflake guildId, int days)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("days", days);

            DiscordApiData data = await rest.Post($"guilds/{guildId}/prune", requestData,
                "guilds/guild/prune").ConfigureAwait(false);
            return data.GetInteger("pruned").Value;
        }

        /// <summary>
        /// Gets a list of integrations for the specified guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrations(Snowflake guildId)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/integrations",
                "guilds/guild/integrations").ConfigureAwait(false);

            DiscordIntegration[] integrations = new DiscordIntegration[data.Values.Count];
            for (int i = 0; i < integrations.Length; i++)
                integrations[i] = new DiscordIntegration(app, data.Values[i], guildId);

            return integrations;
        }

        /// <summary>
        /// Attaches an integration from the current authenticated user to the specified guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task CreateGuildIntegration(Snowflake guildId, Snowflake integrationId, string type)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("id", integrationId);
            requestData.Set("type", type);

            await rest.Post($"guilds/{guildId}/integrations",
                "guilds/guild/integrations").ConfigureAwait(false);
        }

        /// <summary>
        /// Changes the attributes of a guild integration.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task ModifyGuildIntegration(Snowflake guildId, Snowflake integrationId,
            ModifyIntegrationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            await rest.Patch($"guilds/{guildId}/integrations/{integrationId}", requestData,
                "guilds/guild/integrations/integration").ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an attached integration from the specified channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteGuildIntegration(Snowflake guildId, Snowflake integrationId)
        {
            await rest.Delete($"guilds/{guildId}/integrations/{integrationId}",
                "guilds/guild/integrations/integration").ConfigureAwait(false);
        }

        /// <summary>
        /// Synchronizes a guild integration.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task SyncGuildIntegration(Snowflake guildId, Snowflake integrationId)
        {
            await rest.Post($"guilds/{guildId}/integrations/{integrationId}/sync",
                "guilds/guild/integrations/integration/sync").ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the embed for the specified guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildEmbed> GetGuildEmbed(Snowflake guildId)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/embed",
                "guilds/guild/embed").ConfigureAwait(false);
            return new DiscordGuildEmbed(app, guildId, data);
        }

        /// <summary>
        /// Modifies the properties of the embed for the specified guild.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildEmbed> ModifyGuildEmbed(Snowflake guildId, ModifyGuildEmbedParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await rest.Patch($"guilds/{guildId}/embed", requestData,
                "guilds/guild/embed").ConfigureAwait(false);
            return new DiscordGuildEmbed(app, guildId, returnData);
        }
    }
}
