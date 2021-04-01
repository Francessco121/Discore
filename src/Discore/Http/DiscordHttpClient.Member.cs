using Discore.Http.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets a member in a guild by their user ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildMember> GetGuildMember(Snowflake guildId, Snowflake userId)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/members/{userId}",
                $"guilds/{guildId}/members/user").ConfigureAwait(false);
            return new DiscordGuildMember(data, guildId);
        }

        /// <summary>
        /// Gets a member in a guild by their user ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordGuildMember> GetGuildMember(DiscordGuild guild, DiscordUser user)
        {
            return GetGuildMember(guild.Id, user.Id);
        }

        /// <summary>
        /// Gets a list of members in a guild.
        /// This method is paged, and cannot always return every member at once.
        /// </summary>
        /// <param name="guildId">The ID of the guild.</param>
        /// <param name="limit">Max number of members to return (1-1000).</param>
        /// <param name="after">The highest user ID in the previous page.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordGuildMember>> ListGuildMembers(Snowflake guildId,
            int? limit = null, Snowflake? after = null)
        {
            UrlParametersBuilder urlParams = new UrlParametersBuilder();
            urlParams["limit"] = limit?.ToString() ?? null;
            urlParams["after"] = after?.Id.ToString() ?? null;

            DiscordApiData data = await rest.Get($"guilds/{guildId}/members{urlParams.ToQueryString()}",
                $"guilds/{guildId}/members").ConfigureAwait(false);
            DiscordGuildMember[] members = new DiscordGuildMember[data.Values.Count];
            for (int i = 0; i < members.Length; i++)
                members[i] = new DiscordGuildMember(data.Values[i], guildId);

            return members;
        }

        /// <summary>
        /// Gets a list of members in a guild.
        /// This method is paged, and cannot always return every member at once.
        /// </summary>
        /// <param name="guild">The guild.</param>
        /// <param name="limit">Max number of members to return (1-1000).</param>
        /// <param name="after">The highest user ID in the previous page.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordGuildMember>> ListGuildMembers(DiscordGuild guild,
            int? limit = null, Snowflake? after = null)
        {
            return ListGuildMembers(guild.Id, limit, after);
        }

        /// <summary>
        /// Modifies the attributes of a guild member.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task ModifyGuildMember(Snowflake guildId, Snowflake userId, ModifyGuildMemberOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            DiscordApiData requestData = options.Build();

            await rest.Patch($"guilds/{guildId}/members/{userId}", requestData,
                $"guilds/{guildId}/members/user").ConfigureAwait(false);
        }

        /// <summary>
        /// Modifies the attributes of a guild member.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task ModifyGuildMember(DiscordGuildMember member, ModifyGuildMemberOptions options)
        {
            return ModifyGuildMember(member.GuildId, member.Id, options);
        }

        /// <summary>
        /// Modifies the current bot's nickname in the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ChangeNickname"/>.</para>
        /// </summary>
        /// <param name="nickname">The new nickname (or null or an empty string to remove nickname).</param>
        /// <returns>Returns the new nickname (or null if the nickname was removed).</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<string> ModifyCurrentUsersNickname(Snowflake guildId, string nickname)
        {
            DiscordApiData requestData = new DiscordApiData();
            requestData.Set("nick", nickname);

            DiscordApiData returnData = await rest.Patch($"guilds/{guildId}/members/@me/nick", requestData,
                $"guilds/{guildId}/members/@me/nick").ConfigureAwait(false);
            return returnData.GetString("nick");
        }

        /// <summary>
        /// Modifies the current bot's nickname in the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ChangeNickname"/>.</para>
        /// </summary>
        /// <param name="nickname">The new nickname (or null or an empty string to remove nickname).</param>
        /// <returns>Returns the new nickname (or null if the nickname was removed).</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<string> ModifyCurrentUsersNickname(DiscordGuild guild, string nickname)
        {
            return ModifyCurrentUsersNickname(guild.Id, nickname);
        }

        /// <summary>
        /// Removes a member from a guild.
        /// <para>Requires <see cref="DiscordPermission.KickMembers"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task RemoveGuildMember(Snowflake guildId, Snowflake userId)
        {
            await rest.Delete($"guilds/{guildId}/members/{userId}",
                $"guilds/{guildId}/members/user").ConfigureAwait(false);
        }

        /// <summary>
        /// Removes a member from a guild.
        /// <para>Requires <see cref="DiscordPermission.KickMembers"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task RemoveGuildMember(DiscordGuild guild, DiscordUser user)
        {
            return RemoveGuildMember(guild.Id, user.Id);
        }

        /// <summary>
        /// Removes a member from a guild.
        /// <para>Requires <see cref="DiscordPermission.KickMembers"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task RemoveGuildMember(DiscordGuildMember member)
        {
            return RemoveGuildMember(member.GuildId, member.Id);
        }
    }
}
