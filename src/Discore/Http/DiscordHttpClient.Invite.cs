using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Http
{
    partial class DiscordHttpClient
    {
        /// <summary>
        /// Gets an invite by its code.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the invite code is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordInvite> GetInvite(string inviteCode)
        {
            if (inviteCode == null)
                throw new ArgumentNullException(nameof(inviteCode));
            if (string.IsNullOrWhiteSpace(inviteCode))
                throw new ArgumentException("Invite code cannot be empty or only contain whitespace characters.", nameof(inviteCode));

            DiscordApiData data = await rest.Get($"invites/{inviteCode}", "invities/invite").ConfigureAwait(false);
            return new DiscordInvite(this, data);
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

            DiscordApiData data = await rest.Delete($"invites/{inviteCode}", "invities/invite").ConfigureAwait(false);
            return new DiscordInvite(this, data);
        }

        /// <summary>
        /// Gets a list of invites for the specified guild.
        /// <para>Requires <see cref="DiscordPermission.ManageGuild"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordInviteMetadata>> GetGuildInvites(Snowflake guildId)
        {
            DiscordApiData data = await rest.Get($"guilds/{guildId}/invites",
                $"guilds/{guildId}/invites").ConfigureAwait(false);

            DiscordInviteMetadata[] invites = new DiscordInviteMetadata[data.Values.Count];
            for (int i = 0; i < invites.Length; i++)
                invites[i] = new DiscordInviteMetadata(this, data.Values[i]);

            return invites;
        }

        /// <summary>
        /// Gets a list of all invites for the specified guild channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/>.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordInviteMetadata>> GetChannelInvites(Snowflake channelId)
        {
            DiscordApiData data = await rest.Get($"channels/{channelId}/invites",
                $"channels/{channelId}/invites").ConfigureAwait(false);

            DiscordInviteMetadata[] invites = new DiscordInviteMetadata[data.Values.Count];
            for (int i = 0; i < invites.Length; i++)
                invites[i] = new DiscordInviteMetadata(this, data.Values[i]);

            return invites;
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
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            if (maxAge.HasValue) requestData.Set("max_age", maxAge.Value.Seconds);
            if (maxUses.HasValue) requestData.Set("max_uses", maxUses.Value);
            if (temporary.HasValue) requestData.Set("temporary", temporary.Value);
            if (unique.HasValue) requestData.Set("unique", unique.Value);

            DiscordApiData returnData = await rest.Post($"channels/{channelId}/invites", requestData,
                $"channels/{channelId}/invites").ConfigureAwait(false);
            return new DiscordInvite(this, returnData);
        }
    }
}
