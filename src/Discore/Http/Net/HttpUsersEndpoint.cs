using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class HttpUsersEndpoint : HttpApiEndpoint
    {
        public HttpUsersEndpoint(RestClient restClient) 
            : base(restClient)
        { }

        public async Task<DiscordApiData> GetCurrentUser()
        {
            return await Rest.Get("users/@me", "GetCurrentUser");
        }

        public async Task<DiscordApiData> Get(Snowflake id)
        {
            return await Rest.Get($"users/{id}", "GetUser");
        }

        public async Task<DiscordApiData> ModifyCurrentUser(string username = null, DiscordAvatarData avatar = null)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("username", username);
            data.Set("avatar", avatar.ToFormattedString());

            return await Rest.Patch("users/@me", data, "ModifyCurrentUser");
        }

        public async Task<DiscordApiData> GetCurrentUserGuilds()
        {
            return await Rest.Get($"users/@me/guilds", "GetCurrentUserGuilds");
        }

        public async Task<bool> LeaveGuild(Snowflake guildId)
        {
            return (await Rest.Delete($"users/@me/guilds/{guildId}", "LeaveGuild")).IsNull;
        }

        public async Task<DiscordApiData> GetCurrentUserDMs()
        {
            return await Rest.Get("users/@me/channels", "GetCurrentUserDMs");
        }

        public async Task<DiscordApiData> CreateDM(Snowflake recipientId)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("recipient_id", recipientId);

            return await Rest.Post("users/@me/channels", data, "CreateDM");
        }

        public async Task<DiscordApiData> GetCurrentUserConnections()
        {
            return await Rest.Get("users/@me/connections", "GetCurrentUserConnections");
        }
    }
}
