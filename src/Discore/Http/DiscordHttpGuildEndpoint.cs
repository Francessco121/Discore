using Discore.Http.Net;
using Discore.Voice;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    public class DiscordHttpGuildEndpoint : DiscordHttpApiEndpoint
    {
        internal DiscordHttpGuildEndpoint(IDiscordApplication app, RestClient rest) 
            : base(app, rest)
        { }

        /// <summary>
        /// Gets a guild by ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuild> Get(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}", 
                "guilds/guild").ConfigureAwait(false);
            return new DiscordGuild(App, data);
        }

        /// <summary>
        /// Creates a new guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuild> Create(CreateGuildParameters parameters)
        {
            DiscordApiData requestdata = parameters.Build();

            DiscordApiData returnData = await Rest.Post("guilds", requestdata,
                "guilds").ConfigureAwait(false);

            return new DiscordGuild(App, returnData);
        }

        /// <summary>
        /// Changes the settings of a guild.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuild> Modify(Snowflake guildId, ModifyGuildParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}", requestData, 
                "guilds/guild").ConfigureAwait(false);
            return new DiscordGuild(App, returnData);
        }

        /// <summary>
        /// Deletes a guild permanently.
        /// Authenticated user must be the owner.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuild> Delete(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Delete($"guilds/{guildId}", 
                "guilds/guild").ConfigureAwait(false);
            return new DiscordGuild(App, data);
        }

        /// <summary>
        /// Gets a list of all channels in a guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordGuildChannel>> GetChannels(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/channels", 
                "guilds/guild/channels").ConfigureAwait(false);

            DiscordGuildChannel[] channels = new DiscordGuildChannel[data.Values.Count];
            for (int i = 0; i < channels.Length; i++)
                channels[i] = (DiscordGuildChannel)DeserializeChannelData(data.Values[i]);

            return channels;
        }

        /// <summary>
        /// Creates a new text or voice channel for a guild.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildChannel> CreateChannel(Snowflake guildId, CreateGuildChannelParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Post($"guilds/{guildId}/channels", 
                "guilds/guild/channels").ConfigureAwait(false);
            return (DiscordGuildChannel)DeserializeChannelData(returnData);
        }

        /// <summary>
        /// Changes the settings of a channel in a guild.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordGuildChannel>> ModifyChannelPositions(Snowflake guildId, 
            IEnumerable<PositionParameters> positions)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));

            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Array);
            foreach (PositionParameters positionParam in positions)
                requestData.Values.Add(positionParam.Build());

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}/channels", requestData, 
                "guilds/guild/channels").ConfigureAwait(false);

            DiscordGuildChannel[] channels = new DiscordGuildChannel[returnData.Values.Count];
            for (int i = 0; i < channels.Length; i++)
                channels[i] = (DiscordGuildChannel)DeserializeChannelData(returnData.Values[i]);

            return channels;
        }

        /// <summary>
        /// Gets a member in a guild by their user ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildMember> GetMember(Snowflake guildId, Snowflake userId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/members/{userId}", 
                "guilds/guild/members/user").ConfigureAwait(false);
            return new DiscordGuildMember(App, data, guildId);
        }

        /// <summary>
        /// Gets a list of members in a guild.
        /// This method is paged, and cannot always return every member at once.
        /// </summary>
        /// <param name="guildId">The id of the guild.</param>
        /// <param name="limit">Max number of members to return (1-1000).</param>
        /// <param name="after">The highest user id in the previous page.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordGuildMember>> ListMembers(Snowflake guildId, 
            int? limit = null, Snowflake? after = null)
        {
            UrlParametersBuilder urlParams = new UrlParametersBuilder();
            urlParams["limit"] = limit?.ToString() ?? null;
            urlParams["after"] = after?.Id.ToString() ?? null;

            DiscordApiData data = await Rest.Get($"guilds/{guildId}/members{urlParams.ToQueryString()}", 
                "guilds/guild/members").ConfigureAwait(false);
            DiscordGuildMember[] members = new DiscordGuildMember[data.Values.Count];
            for (int i = 0; i < members.Length; i++)
                members[i] = new DiscordGuildMember(App, data.Values[i], guildId);

            return members;
        }

        /// <summary>
        /// Modifies the attributes of a guild member.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> ModifyMember(Snowflake guildId, Snowflake userId, ModifyGuildMemberParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}/members/{userId}", requestData, 
                "guilds/guild/members/user").ConfigureAwait(false);
            return requestData.IsNull;
        }

        /// <summary>
        /// Modifies the current authenticated user's nickname in the specified guild.
        /// </summary>
        /// <param name="nickname">The new nickname (or null or an empty string to remove nickname).</param>
        /// <returns>Returns the new nickname (or null if the nickname was removed).</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<string> ModifyCurrentUsersNickname(Snowflake guildId, string nickname)
        {
            DiscordApiData requestData = new DiscordApiData();
            requestData.Set("nick", nickname);

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}/members/@me/nick", requestData,
                "guilds/guild/members/@me/nick").ConfigureAwait(false);
            return returnData.GetString("nick");
        }

        /// <summary>
        /// Adds a role to a guild member.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> AddMemberRole(Snowflake guildId, Snowflake userId, Snowflake roleId)
        {
            DiscordApiData data = await Rest.Put($"guilds/{guildId}/members/{userId}/roles/{roleId}",
                "guilds/guild/members/member/roles/role").ConfigureAwait(false);
            return data.IsNull;
        }

        /// <summary>
        /// Removes a role from a guild member.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> RemoveMemberRole(Snowflake guildId, Snowflake userId, Snowflake roleId)
        {
            DiscordApiData data = await Rest.Delete($"guilds/{guildId}/members/{userId}/roles/{roleId}",
                "guilds/guild/members/member/roles/role").ConfigureAwait(false);
            return data.IsNull;
        }

        /// <summary>
        /// Removes a member from a guild.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> RemoveMember(Snowflake guildId, Snowflake userId)
        {
            DiscordApiData data = await Rest.Delete($"guilds/{guildId}/members/{userId}", 
                "guilds/guild/members/user").ConfigureAwait(false);
            return data.IsNull;
        }

        /// <summary>
        /// Gets a list of all users that are banned from the specified guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordUser>> GetBans(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/bans", 
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
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> CreateBan(Snowflake guildId, Snowflake userId, int? deleteMessageDays = null)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("delete-message-days", deleteMessageDays);

            DiscordApiData returnData = await Rest.Put($"guilds/{guildId}/bans/{userId}", requestData, 
                "guilds/guild/bans/user").ConfigureAwait(false);
            return returnData.IsNull;
        }

        /// <summary>
        /// Removes a user ban from the specified guild.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> RemoveBan(Snowflake guildId, Snowflake userId)
        {
            DiscordApiData data = await Rest.Delete($"guilds/{guildId}/bans/{userId}", 
                "guilds/guild/bans/user").ConfigureAwait(false);
            return data.IsNull;
        }

        /// <summary>
        /// Gets a list of all roles in a guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordRole>> GetRoles(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/roles", 
                "guilds/guild/roles").ConfigureAwait(false);

            DiscordRole[] roles = new DiscordRole[data.Values.Count];
            for (int i = 0; i < roles.Length; i++)
                roles[i] = new DiscordRole(App, guildId, data.Values[i]);

            return roles;
        }

        /// <summary>
        /// Creates a new role with default settings for a guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordRole> CreateRole(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Post($"guilds/{guildId}/roles", 
                "guilds/guild/roles").ConfigureAwait(false);
            return new DiscordRole(App, guildId, data);
        }

        /// <summary>
        /// Creates a new role for a guild.
        /// </summary>
        /// <param name="parameters">A set of optional parameters to use when creating the role.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordRole> CreateRole(Snowflake guildId, CreateRoleParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Post($"guilds/{guildId}/roles", requestData,
                "guilds/guild/roles").ConfigureAwait(false);
            return new DiscordRole(App, guildId, returnData);
        }

        /// <summary>
        /// Modifies the sorting positions of roles in a guild.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordRole>> ModifyRolePositions(Snowflake guildId, 
            IEnumerable<PositionParameters> positions)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));

            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Array);
            foreach (PositionParameters positionParam in positions)
                requestData.Values.Add(positionParam.Build());

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}/roles", requestData, 
                "guilds/guild/roles").ConfigureAwait(false);

            DiscordRole[] roles = new DiscordRole[returnData.Values.Count];
            for (int i = 0; i < roles.Length; i++)
                roles[i] = new DiscordRole(App, guildId, returnData.Values[i]);

            return roles;
        }

        /// <summary>
        /// Modifies the settings of a guild role.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordRole> ModifyRole(Snowflake guildId, Snowflake roleId, ModifyRoleParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}/roles/{roleId}", requestData, 
                "guilds/guild/roles/role").ConfigureAwait(false);
            return new DiscordRole(App, guildId, returnData);
        }

        /// <summary>
        /// Deletes a role from a guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> DeleteRole(Snowflake guildId, Snowflake roleId)
        {
            DiscordApiData data = await Rest.Delete($"guilds/{guildId}/roles/{roleId}", 
                "guilds/guild/roles/role").ConfigureAwait(false);
            return data.IsNull;
        }

        /// <summary>
        /// Returns the number of members that would be kicked from a guild prune operation.
        /// </summary>
        /// <param name="days">The number of days to count prune for (1 or more).</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<int> GetPruneCount(Snowflake guildId, int days)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/prune?days={days}", 
                "guilds/guild/prune").ConfigureAwait(false);
            return data.GetInteger("pruned").Value;
        }

        /// <summary>
        /// Begins a member prune operation, 
        /// kicking every member that has been offline for the specified number of days.
        /// </summary>
        /// <param name="days">The number of days to prune (1 or more).</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<int> BeginPrune(Snowflake guildId, int days)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("days", days);

            DiscordApiData data = await Rest.Post($"guilds/{guildId}/prune", requestData, 
                "guilds/guild/prune").ConfigureAwait(false);
            return data.GetInteger("pruned").Value;
        }

        /// <summary>
        /// Gets a list of all voice regions available to the specified guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordVoiceRegion>> GetVoiceRegions(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/regions", 
                "guilds/guild/regions").ConfigureAwait(false);

            DiscordVoiceRegion[] regions = new DiscordVoiceRegion[data.Values.Count];
            for (int i = 0; i < regions.Length; i++)
                regions[i] = new DiscordVoiceRegion(data.Values[i]);

            return regions;
        }

        /// <summary>
        /// Gets a list of invites for the specified guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordInviteMetadata>> GetInvites(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/invites", 
                "guilds/guild/invites").ConfigureAwait(false);

            DiscordInviteMetadata[] invites = new DiscordInviteMetadata[data.Values.Count];
            for (int i = 0; i < invites.Length; i++)
                invites[i] = new DiscordInviteMetadata(App, data.Values[i]);

            return invites;
        }

        /// <summary>
        /// Gets a list of integrations for the specified guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordIntegration>> GetIntegrations(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/integrations", 
                "guilds/guild/integrations").ConfigureAwait(false);

            DiscordIntegration[] integrations = new DiscordIntegration[data.Values.Count];
            for (int i = 0; i < integrations.Length; i++)
                integrations[i] = new DiscordIntegration(App, data.Values[i], guildId);

            return integrations;
        }

        /// <summary>
        /// Attaches an integration from the current authenticated user to the specified guild.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> CreateIntegration(Snowflake guildId, Snowflake integrationId, string type)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("id", integrationId);
            requestData.Set("type", type);

            DiscordApiData returnData = await Rest.Post($"guilds/{guildId}/integrations",
                "guilds/guild/integrations").ConfigureAwait(false);
            return returnData.IsNull;
        }

        /// <summary>
        /// Changes the attributes of a guild integration.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> ModifyIntegration(Snowflake guildId, Snowflake integrationId, 
            ModifyIntegrationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}/integrations/{integrationId}", requestData,
                "guilds/guild/integrations/integration").ConfigureAwait(false);
            return returnData.IsNull;
        }

        /// <summary>
        /// Deletes an attached integration from the specified channel.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> DeleteIntegration(Snowflake guildId, Snowflake integrationId)
        {
            DiscordApiData data = await Rest.Delete($"guilds/{guildId}/integrations/{integrationId}",
                "guilds/guild/integrations/integration").ConfigureAwait(false);
            return data.IsNull;
        }

        /// <summary>
        /// Synchronizes a guild integration.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> SyncIntegration(Snowflake guildId, Snowflake integrationId)
        {
            DiscordApiData data = await Rest.Post($"guilds/{guildId}/integrations/{integrationId}/sync",
                "guilds/guild/integrations/integration/sync").ConfigureAwait(false);
            return data.IsNull;
        }

        /// <summary>
        /// Returns the embed for the specified guild.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildEmbed> GetEmbed(Snowflake guildId)
        {
            DiscordApiData data = await Rest.Get($"guilds/{guildId}/embed", 
                "guilds/guild/embed").ConfigureAwait(false);
            return new DiscordGuildEmbed(App, guildId, data);
        }

        /// <summary>
        /// Modifies the properties of the embed for the specified guild.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildEmbed> ModifyEmbed(Snowflake guildId, ModifyGuildEmbedParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Patch($"guilds/{guildId}/embed", requestData,
                "guilds/guild/embed").ConfigureAwait(false);
            return new DiscordGuildEmbed(App, guildId, returnData);
        }
    }
}
