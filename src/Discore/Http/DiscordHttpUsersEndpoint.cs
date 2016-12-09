using Discore.Http.Net;
using System.Threading.Tasks;

namespace Discore.Http
{
    public sealed class DiscordHttpUsersEndpoint
    {
        IDiscordApplication app;
        HttpUsersEndpoint endpoint;

        internal DiscordHttpUsersEndpoint(IDiscordApplication app, HttpUsersEndpoint endpoint)
        {
            this.app = app;
            this.endpoint = endpoint;
        }

        public async Task<DiscordUser> GetCurrentUser()
        {
            DiscordApiData data = await endpoint.GetCurrentUser();
            return new DiscordUser(data);
        }

        public async Task<DiscordUser> Get(Snowflake id)
        {
            DiscordApiData data = await endpoint.Get(id);
            return new DiscordUser(data);
        }

        public async Task<DiscordUser> ModifyCurrentUser(string username = null, DiscordAvatarData avatar = null)
        {
            DiscordApiData data = await endpoint.ModifyCurrentUser(username, avatar);
            return data.IsNull ? null : new DiscordUser(data);
        }

        public async Task<DiscordUserGuild[]> GetCurrentUserGuilds()
        {
            DiscordApiData data = await endpoint.GetCurrentUserGuilds();
            DiscordUserGuild[] guilds = new DiscordUserGuild[data.Values.Count];

            for (int i = 0; i < guilds.Length; i++)
                guilds[i] = new DiscordUserGuild(data.Values[i]);

            return guilds;
        }

        public async Task<bool> LeaveGuild(Snowflake guildId)
        {
            return await endpoint.LeaveGuild(guildId);
        }

        public async Task<DiscordDMChannel[]> GetCurrentUserDMs()
        {
            DiscordApiData data = await endpoint.GetCurrentUserDMs();
            DiscordDMChannel[] dms = new DiscordDMChannel[data.Values.Count];

            for (int i = 0; i < dms.Length; i++)
                dms[i] = new DiscordDMChannel(app, data.Values[i]);

            return dms;
        }

        public async Task<DiscordConnection[]> GetCurrentUserConnections()
        {
            DiscordApiData data = await endpoint.GetCurrentUserConnections();
            DiscordConnection[] connections = new DiscordConnection[data.Values.Count];

            for (int i = 0; i < connections.Length; i++)
                connections[i] = new DiscordConnection(data.Values[i]);

            return connections;
        }
    }
}
