using Discore.Http.Net;

namespace Discore.Http
{
    public sealed class DiscordHttpApi
    {
        public DiscordHttpUsersEndpoint Users { get; }

        internal DiscordHttpApi(HttpApi api)
        {
            Users = new DiscordHttpUsersEndpoint(api.Users);
        }
    }
}
