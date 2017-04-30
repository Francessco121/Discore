using Discore.Http.Net;
using System;
using System.Threading.Tasks;

namespace Discore.Http
{
    public class DiscordHttpInviteEndpoint : DiscordHttpApiEndpoint
    {
        internal DiscordHttpInviteEndpoint(IDiscordApplication app, RestClient rest)
            : base(app, rest)
        { }

        /// <summary>
        /// Gets an invite by its code.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the invite code is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordInvite> Get(string inviteCode)
        {
            if (inviteCode == null)
                throw new ArgumentNullException(nameof(inviteCode));
            if (string.IsNullOrWhiteSpace(inviteCode))
                throw new ArgumentException("Invite code cannot be empty or only contain whitespace characters.", nameof(inviteCode));

            DiscordApiData data = await Rest.Get($"invites/{inviteCode}", "invities/invite").ConfigureAwait(false);
            return new DiscordInvite(App, data);
        }

        /// <summary>
        /// Deletes an invite to a channel.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the invite code is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordInvite> Delete(string inviteCode)
        {
            if (inviteCode == null)
                throw new ArgumentNullException(nameof(inviteCode));
            if (string.IsNullOrWhiteSpace(inviteCode))
                throw new ArgumentException("Invite code cannot be empty or only contain whitespace characters.", nameof(inviteCode));

            DiscordApiData data = await Rest.Delete($"invites/{inviteCode}", "invities/invite").ConfigureAwait(false);
            return new DiscordInvite(App, data);
        }

        /// <summary>
        /// Accepts an invite to a channel.
        /// Note: This does not work for bot accounts.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the invite code is empty or only contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordInvite> Accept(string inviteCode)
        {
            if (inviteCode == null)
                throw new ArgumentNullException(nameof(inviteCode));
            if (string.IsNullOrWhiteSpace(inviteCode))
                throw new ArgumentException("Invite code cannot be empty or only contain whitespace characters.", nameof(inviteCode));

            DiscordApiData data = await Rest.Post($"invites/{inviteCode}", "invities/invite").ConfigureAwait(false);
            return new DiscordInvite(App, data);
        }
    }
}
