using Discore.Http.Internal;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

#nullable enable

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets an invite by its code.
        /// </summary>
        /// <param name="withCounts">Whether the returned invite should contain approximate member counts.</param>
        /// <exception cref="ArgumentException">Thrown if the invite code is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordInvite> GetInvite(string inviteCode, bool? withCounts = null)
        {
            if (inviteCode == null)
                throw new ArgumentNullException(nameof(inviteCode));
            if (string.IsNullOrWhiteSpace(inviteCode))
                throw new ArgumentException("Invite code cannot be empty or only contain whitespace characters.", nameof(inviteCode));

            var urlParams = new UrlParametersBuilder();
            urlParams["with_counts"] = withCounts?.ToString() ?? null;

            using JsonDocument? data = await rest.Get($"invites/{inviteCode}{urlParams.ToQueryString()}", "invities/invite").ConfigureAwait(false);
            return new DiscordInvite(data!.RootElement);
        }

        /// <summary>
        /// Deletes an invite to a channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the invite code is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordInvite> DeleteInvite(string inviteCode)
        {
            if (inviteCode == null)
                throw new ArgumentNullException(nameof(inviteCode));
            if (string.IsNullOrWhiteSpace(inviteCode))
                throw new ArgumentException("Invite code cannot be empty or only contain whitespace characters.", nameof(inviteCode));

            using JsonDocument? data = await rest.Delete($"invites/{inviteCode}", "invities/invite").ConfigureAwait(false);
            return new DiscordInvite(data!.RootElement);
        }

        /// <summary>
        /// Deletes an invite to a channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the invite code is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordInvite> DeleteInvite(DiscordInvite invite)
        {
            return DeleteInvite(invite.Code);
        }

        /// <summary>
        /// Gets a list of invites for the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordInviteMetadata>> GetGuildInvites(Snowflake guildId)
        {
            using JsonDocument? data = await rest.Get($"guilds/{guildId}/invites",
                $"guilds/{guildId}/invites").ConfigureAwait(false);

            JsonElement values = data!.RootElement;

            var invites = new DiscordInviteMetadata[values.GetArrayLength()];
            for (int i = 0; i < invites.Length; i++)
                invites[i] = new DiscordInviteMetadata(values[i]);

            return invites;
        }

        /// <summary>
        /// Gets a list of invites for the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordInviteMetadata>> GetGuildInvites(DiscordGuild guild)
        {
            return GetGuildInvites(guild.Id);
        }

        /// <summary>
        /// Gets a list of all invites for the specified guild channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordInviteMetadata>> GetChannelInvites(Snowflake channelId)
        {
            using JsonDocument? data = await rest.Get($"channels/{channelId}/invites",
                $"channels/{channelId}/invites").ConfigureAwait(false);

            JsonElement values = data!.RootElement;

            var invites = new DiscordInviteMetadata[values.GetArrayLength()];
            for (int i = 0; i < invites.Length; i++)
                invites[i] = new DiscordInviteMetadata(values[i]);

            return invites;
        }

        /// <summary>
        /// Gets a list of all invites for the specified guild channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordInviteMetadata>> GetChannelInvites(DiscordGuildChannel channel)
        {
            return GetChannelInvites(channel.Id);
        }

        /// <summary>
        /// Creates a new invite for the specified guild channel.
        /// <para>Requires <see cref="DiscordPermission.CreateInstantInvite"/>.</para>
        /// </summary>
        /// <param name="channelId">The ID of the guild channel.</param>
        /// <param name="maxAge">The duration of invite before expiry, or 0 or null for never.</param>
        /// <param name="maxUses">The max number of uses or 0 or null for unlimited.</param>
        /// <param name="temporary">Whether this invite only grants temporary membership.</param>
        /// <param name="unique">
        /// If true, don't try to reuse a similar invite 
        /// (useful for creating many unique one time use invites).
        /// </param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordInvite> CreateChannelInvite(Snowflake channelId,
            TimeSpan? maxAge = null, int? maxUses = null, bool? temporary = null, bool? unique = null)
        {
            string requestData = BuildJsonContent(writer =>
            {
                writer.WriteStartObject();

                if (maxAge.HasValue) writer.WriteNumber("max_age", maxAge.Value.Seconds);
                if (maxUses.HasValue) writer.WriteNumber("max_uses", maxUses.Value);
                if (temporary.HasValue) writer.WriteBoolean("temporary", temporary.Value);
                if (unique.HasValue) writer.WriteBoolean("unique", unique.Value);

                writer.WriteEndObject();
            });

            using JsonDocument? returnData = await rest.Post($"channels/{channelId}/invites", requestData,
                $"channels/{channelId}/invites").ConfigureAwait(false);

            return new DiscordInvite(returnData!.RootElement);
        }

        /// <summary>
        /// Creates a new invite for the specified guild channel.
        /// <para>Requires <see cref="DiscordPermission.CreateInstantInvite"/>.</para>
        /// </summary>
        /// <param name="channel">The guild channel.</param>
        /// <param name="maxAge">The duration of invite before expiry, or 0 or null for never.</param>
        /// <param name="maxUses">The max number of uses or 0 or null for unlimited.</param>
        /// <param name="temporary">Whether this invite only grants temporary membership.</param>
        /// <param name="unique">
        /// If true, don't try to reuse a similar invite 
        /// (useful for creating many unique one time use invites).
        /// </param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordInvite> CreateChannelInvite(DiscordGuildChannel channel,
            TimeSpan? maxAge = null, int? maxUses = null, bool? temporary = null, bool? unique = null)
        {
            return CreateChannelInvite(channel.Id,
                maxAge: maxAge,
                maxUses: maxUses,
                temporary: temporary,
                unique: unique);
        }
    }
}

#nullable restore
