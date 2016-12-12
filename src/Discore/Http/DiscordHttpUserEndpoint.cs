using Discore.Http.Net;
using System.Threading.Tasks;

namespace Discore.Http
{
    public sealed class DiscordHttpUserEndpoint : DiscordHttpApiEndpoint
    {
        internal DiscordHttpUserEndpoint(IDiscordApplication app, RestClient rest)
            : base(app, rest)
        { }

        public async Task<DiscordUser> GetCurrentUser()
        {
            DiscordApiData data = await Rest.Get("users/@me", "GetCurrentUser");
            return new DiscordUser(data);
        }

        public async Task<DiscordUser> Get(Snowflake id)
        {
            DiscordApiData data = await Rest.Get($"users/{id}", "GetUser");
            return new DiscordUser(data);
        }

        public async Task<DiscordUser> ModifyCurrentUser(string username = null, DiscordAvatarData avatar = null)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("username", username);
            requestData.Set("avatar", avatar.ToFormattedString());

            DiscordApiData returnData = await Rest.Patch("users/@me", requestData, "ModifyCurrentUser");
            return returnData.IsNull ? null : new DiscordUser(returnData);
        }

        public async Task<DiscordUserGuild[]> GetCurrentUserGuilds()
        {
            DiscordApiData data = await Rest.Get($"users/@me/guilds", "GetCurrentUserGuilds");
            DiscordUserGuild[] guilds = new DiscordUserGuild[data.Values.Count];

            for (int i = 0; i < guilds.Length; i++)
                guilds[i] = new DiscordUserGuild(data.Values[i]);

            return guilds;
        }

        public async Task<bool> LeaveGuild(Snowflake guildId)
        {
            return (await Rest.Delete($"users/@me/guilds/{guildId}", "LeaveGuild")).IsNull;
        }

        public async Task<DiscordDMChannel[]> GetCurrentUserDMs()
        {
            DiscordApiData data = await Rest.Get("users/@me/channels", "GetCurrentUserDMs");
            DiscordDMChannel[] dms = new DiscordDMChannel[data.Values.Count];

            for (int i = 0; i < dms.Length; i++)
                dms[i] = new DiscordDMChannel(App, data.Values[i]);

            return dms;
        }

        public async Task<DiscordDMChannel> CreateDM(Snowflake recipientId)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("recipient_id", recipientId);

            DiscordApiData returnData = await Rest.Post("users/@me/channels", requestData, "CreateDM");
            return new DiscordDMChannel(App, returnData);
        }

        public async Task<DiscordConnection[]> GetCurrentUserConnections()
        {
            DiscordApiData data = await Rest.Get("users/@me/connections", "GetCurrentUserConnections");
            DiscordConnection[] connections = new DiscordConnection[data.Values.Count];

            for (int i = 0; i < connections.Length; i++)
                connections[i] = new DiscordConnection(App, data.Values[i]);

            return connections;
        }
    }
}
