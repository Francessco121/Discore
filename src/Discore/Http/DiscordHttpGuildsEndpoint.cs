using Discore.Http.Net;
using Discore.Voice;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    public class DiscordHttpGuildsEndpoint : DiscordHttpApiEndpoint
    {
        internal DiscordHttpGuildsEndpoint(IDiscordApplication app, RestClient rest) 
            : base(app, rest)
        { }

        /// <summary>
        /// Gets a guild by ID.
        /// </summary>
        public async Task<DiscordGuild> Get(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}", "GetGuild");
            return new DiscordGuild(data);
        }

        /// <summary>
        /// Changes the settings of a guild.
        /// </summary>
        public async Task<DiscordGuild> Modify(Snowflake guildId, ModifyGuildParameters parameters)
        {
            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}", requestData, "ModifyGuild");
            return new DiscordGuild(returnData);
        }

        /// <summary>
        /// Deletes a guild permanently.
        /// Authenticated user must be the owner.
        /// </summary>
        public async Task<DiscordGuild> Delete(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Delete($"guilds/{guildId}", "DeleteGuild");
            return new DiscordGuild(data);
        }

        /// <summary>
        /// Gets a list of all channels in a guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordGuildChannel>> GetGuildChannels(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/channels", "GetGuildChannels");

            DiscordGuildChannel[] channels = new DiscordGuildChannel[data.Values.Count];
            for (int i = 0; i < channels.Length; i++)
                channels[i] = (DiscordGuildChannel)GetChannelAsProperChannel(data.Values[i]);

            return channels;
        }

        /// <summary>
        /// Creates a new text or voice channel for a guild.
        /// </summary>
        public async Task<DiscordGuildChannel> CreateGuildChannel(Snowflake guildId, CreateGuildChannelParameters parameters)
        {
            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Post($"guilds/{guildId}/channels", "CreateGuildChannel");
            return (DiscordGuildChannel)GetChannelAsProperChannel(returnData);
        }

        /// <summary>
        /// Changes the settings of a channel in a guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordGuildChannel>> ModifyGuildChannelPositions(Snowflake guildId, 
            IEnumerable<PositionParameters> positions)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Array);
            foreach (PositionParameters positionParam in positions)
                requestData.Values.Add(positionParam.Build());

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}/channels", requestData, "ModifyGuildChannelPositions");

            DiscordGuildChannel[] channels = new DiscordGuildChannel[returnData.Values.Count];
            for (int i = 0; i < channels.Length; i++)
                channels[i] = (DiscordGuildChannel)GetChannelAsProperChannel(returnData.Values[i]);

            return channels;
        }

        /// <summary>
        /// Gets a member in a guild by their user ID.
        /// </summary>
        public async Task<DiscordGuildMember> GetGuildMember(Snowflake guildId, Snowflake userId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/members/{userId}", "GetGuildMember");
            return new DiscordGuildMember(data, guildId);
        }

        /// <summary>
        /// Gets a list of members in a guild.
        /// This method is paged, and cannot always return every member at once.
        /// </summary>
        /// <param name="guildId">The id of the guild.</param>
        /// <param name="limit">Max number of members to return (1-1000).</param>
        /// <param name="after">The highest user id in the previous page.</param>
        public async Task<IReadOnlyList<DiscordGuildMember>> ListGuildMembers(Snowflake guildId, 
            int? limit = null, Snowflake? after = null)
        {
            UrlParametersBuilder urlParams = new UrlParametersBuilder();
            urlParams["limit"] = limit?.ToString() ?? null;
            // TODO: This needs testing, they specify after as "a user id" but as the type integer instead of string.
            // Could just be an issue with the documentation.
            urlParams["after"] = after?.Id.ToString() ?? null;

            DiscordApiData data = await Rest.Get($"guilds/{guildId}/members{urlParams.ToQueryString()}", "ListGuildMembers");
            DiscordGuildMember[] members = new DiscordGuildMember[data.Values.Count];
            for (int i = 0; i < members.Length; i++)
                members[i] = new DiscordGuildMember(data.Values[i], guildId);

            return members;
        }

        /// <summary>
        /// Modifies the attributes of a guild member.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> ModifyGuildMember(Snowflake guildId, Snowflake userId, ModifyGuildMemberParameters parameters)
        {
            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}/members/{userId}", requestData, "ModifyGuildMember");
            return requestData.IsNull;
        }

        /// <summary>
        /// Removes a member from a guild.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> RemoveGuildMember(Snowflake guildId, Snowflake userId)
        {
            DiscordApiData data = await Rest.Delete($"guilds/{guildId}/members/{userId}", "RemoveGuildMember");
            return data.IsNull;
        }

        /// <summary>
        /// Gets a list of all users that are banned from the specified guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordUser>> GetGuildBans(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/bans", "GetGuildBans");

            DiscordUser[] users = new DiscordUser[data.Values.Count];
            for (int i = 0; i < users.Length; i++)
                users[i] = new DiscordUser(data.Values[i]);

            return users;
        }

        /// <summary>
        /// Bans a users from the specified guild.
        /// </summary>
        /// <param name="deleteMessageDays">Number of days to delete messages for (0-7).</param>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> CreateGuildBan(Snowflake guildId, Snowflake userId, int? deleteMessageDays = null)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("delete-message-days", deleteMessageDays);

            DiscordApiData returnData = await Rest.Put($"guilds/{guildId}/bans/{userId}", requestData, "CreateGuildBan");
            return returnData.IsNull;
        }

        /// <summary>
        /// Removes a user ban from the specified guild.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> RemoveGuildBan(Snowflake guildId, Snowflake userId)
        {
            DiscordApiData data = await Rest.Delete($"guilds/{guildId}/bans/{userId}", "RemoveGuildBan");
            return data.IsNull;
        }

        /// <summary>
        /// Gets a list of all roles in a guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordRole>> GetGuildRoles(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/roles", "GetGuildRoles");

            DiscordRole[] roles = new DiscordRole[data.Values.Count];
            for (int i = 0; i < roles.Length; i++)
                roles[i] = new DiscordRole(data.Values[i]);

            return roles;
        }

        /// <summary>
        /// Creates a new role with default settings for a guild.
        /// </summary>
        public async Task<DiscordRole> CreateGuildRole(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Post($"guilds/{guildId}/roles", "CreateGuildRole");
            return new DiscordRole(data);
        }

        /// <summary>
        /// Modifies the sorting positions of roles in a guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordRole>> ModifyGuildRolePositions(Snowflake guildId, 
            IEnumerable<PositionParameters> positions)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Array);
            foreach (PositionParameters positionParam in positions)
                requestData.Values.Add(positionParam.Build());

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}/roles", requestData, "ModifyGuildRolePositions");

            DiscordRole[] roles = new DiscordRole[returnData.Values.Count];
            for (int i = 0; i < roles.Length; i++)
                roles[i] = new DiscordRole(returnData.Values[i]);

            return roles;
        }

        /// <summary>
        /// Modifies the settings of a guild role.
        /// </summary>
        public async Task<DiscordRole> ModifyGuildRole(Snowflake guildId, Snowflake roleId, ModifyRoleParameters parameters)
        {
            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}/roles/{roleId}", requestData, "ModifyGuildRole");
            return new DiscordRole(returnData);
        }

        /// <summary>
        /// Deletes a role from a guild.
        /// </summary>
        public async Task<DiscordRole> DeleteGuildRole(Snowflake guildId, Snowflake roleId)
        {
            DiscordApiData data = await Rest.Delete($"guilds/{guildId}/roles/{roleId}", "DeleteGuildRole");
            return new DiscordRole(data);
        }

        /// <summary>
        /// Returns the number of members that would be kicked from a guild prune operation.
        /// </summary>
        /// <param name="days">The number of days to count prune for (1 or more).</param>
        public async Task<int> GetGuildPruneCount(Snowflake guildId, int days)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/prune?days={days}", "GetGuildPruneCount");
            return data.GetInteger("pruned").Value;
        }

        /// <summary>
        /// Begins a member prune operation, 
        /// kicking every member that has been offline for the specified number of days.
        /// </summary>
        /// <param name="days">The number of days to prune (1 or more).</param>
        public async Task<int> BeginGuildPrune(Snowflake guildId, int days)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("days", days);

            DiscordApiData data = await Rest.Post($"guilds/{guildId}/prune", requestData, "BeginGuildPrune");
            return data.GetInteger("pruned").Value;
        }

        /// <summary>
        /// Gets a list of all voice regions available to this guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordVoiceRegion>> GetGuildVoiceRegions(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/regions", "GetGuildVoiceRegions");

            DiscordVoiceRegion[] regions = new DiscordVoiceRegion[data.Values.Count];
            for (int i = 0; i < regions.Length; i++)
                regions[i] = new DiscordVoiceRegion(data.Values[i]);

            return regions;
        }

        /// <summary>
        /// Gets a list of invites for the specified guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordInviteMetadata>> GetGuildInvites(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/invites", "GetGuildInvites");

            DiscordInviteMetadata[] invites = new DiscordInviteMetadata[data.Values.Count];
            for (int i = 0; i < invites.Length; i++)
                invites[i] = new DiscordInviteMetadata(data.Values[i]);

            return invites;
        }

        /// <summary>
        /// Gets a list of integrations for the specified guild.
        /// </summary>
        public async Task<IReadOnlyList<DiscordIntegration>> GetGuildIntegrations(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/integrations", "GetGuildIntegrations");

            DiscordIntegration[] integrations = new DiscordIntegration[data.Values.Count];
            for (int i = 0; i < integrations.Length; i++)
                integrations[i] = new DiscordIntegration(data.Values[i], guildId);

            return integrations;
        }

        /// <summary>
        /// Attaches an integration from the current authenticated user to the specified guild.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> CreateGuildIntegration(Snowflake guildId, Snowflake integrationId, string type)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("id", integrationId);
            requestData.Set("type", type);

            DiscordApiData returnData = await Rest.Post($"guilds/{guildId}/integrations", "CreateGuildIntegration");
            return returnData.IsNull;
        }

        /// <summary>
        /// Changes the attributes of a guild integration.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> ModifyGuildIntegration(Snowflake guildId, Snowflake integrationId, 
            ModifyIntegrationParameters parameters)
        {
            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}/integrations/{integrationId}", requestData, 
                "ModifyGuildIntegration");
            return returnData.IsNull;
        }

        /// <summary>
        /// Deletes an attached integration from the specified channel.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> DeleteGuildIntegration(Snowflake guildId, Snowflake integrationId)
        {
            DiscordApiData data = await Rest.Delete($"guilds/{guildId}/integrations/{integrationId}", "DeleteGuildIntegration");
            return data.IsNull;
        }

        /// <summary>
        /// Synchronizes a guild integration.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> SyncGuildIntegration(Snowflake guildId, Snowflake integrationId)
        {
            DiscordApiData data = await Rest.Post($"guilds/{guildId}/integrations/{integrationId}/sync", "SyncGuildIntegration");
            return data.IsNull;
        }

        /// <summary>
        /// Returns the embed for the specified guild.
        /// </summary>
        public async Task<DiscordGuildEmbed> GetGuildEmbed(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/embed", "GetGuildEmbed");
            return new DiscordGuildEmbed(guildId, data);
        }

        /// <summary>
        /// Modifies the properties of the embed for the specified guild.
        /// </summary>
        public async Task<DiscordGuildEmbed> ModifyGuildEmbed(Snowflake guildId, ModifyGuildEmbedParameters parameters)
        {
            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}/embed", requestData, "GetGuildEmbed");
            return new DiscordGuildEmbed(guildId, returnData);
        }
    }
}
