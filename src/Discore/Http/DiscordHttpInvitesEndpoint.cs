using Discore.Http.Net;
using System.Threading.Tasks;

namespace Discore.Http
{
    public class DiscordHttpInvitesEndpoint : DiscordHttpApiEndpoint
    {
        internal DiscordHttpInvitesEndpoint(IDiscordApplication app, RestClient rest)
            : base(app, rest)
        { }

        public async Task<DiscordInvite> Get(string inviteCode)
        {
            DiscordApiData data = await Rest.Get($"invites/{inviteCode}", "GetInvite");
            return new DiscordInvite(data);
        }

        public async Task<DiscordInvite> Delete(string inviteCode)
        {
            DiscordApiData data = await Rest.Delete($"invites/{inviteCode}", "DeleteInvite");
            return new DiscordInvite(data);
        }

        public async Task<DiscordInvite> Accept(string inviteCode)
        {
            DiscordApiData data = await Rest.Post($"invites/{inviteCode}", "AcceptInvite");
            return new DiscordInvite(data);
        }
    }
}
