using Discore.Http.Net;
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
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordInvite> Get(string inviteCode)
        {
            DiscordApiData data = await Rest.Get($"invites/{inviteCode}", "GetInvite").ConfigureAwait(false);
            return new DiscordInvite(App, data);
        }

        /// <summary>
        /// Deletes an invite to a channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordInvite> Delete(string inviteCode)
        {
            DiscordApiData data = await Rest.Delete($"invites/{inviteCode}", "DeleteInvite").ConfigureAwait(false);
            return new DiscordInvite(App, data);
        }

        /// <summary>
        /// Accepts an invite to a channel.
        /// Note: This does not work for bot accounts.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordInvite> Accept(string inviteCode)
        {
            DiscordApiData data = await Rest.Post($"invites/{inviteCode}", "AcceptInvite").ConfigureAwait(false);
            return new DiscordInvite(App, data);
        }
    }
}
