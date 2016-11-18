using Discore.Http.Net;

namespace Discore.Http
{
    public sealed class DiscordHttpUsersEndpoint
    {
        HttpUsersEndpoint endpoint;

        internal DiscordHttpUsersEndpoint(HttpUsersEndpoint endpoint)
        {
            this.endpoint = endpoint;
        }

        public DiscordUser GetCurrentUser()
        {
            DiscordApiData data = endpoint.GetCurrentUser();
            return new DiscordUser(data);
        }

        public DiscordUser Get(Snowflake id)
        {
            DiscordApiData data = endpoint.Get(id);
            return new DiscordUser(data);
        }

        public DiscordUser ModifyCurrentUser(string username = null, DiscordAvatarData avatar = null)
        {
            DiscordApiData data = endpoint.ModifyCurrentUser(username, avatar);
            return data.IsNull ? null : new DiscordUser(data);
        }

        public DiscordUserGuild[] GetCurrentUserGuilds()
        {
            DiscordApiData data = endpoint.GetCurrentUserGuilds();
            DiscordUserGuild[] guilds = new DiscordUserGuild[data.Values.Count];

            for (int i = 0; i < guilds.Length; i++)
                guilds[i] = new DiscordUserGuild(data.Values[i]);

            return guilds;
        }

        public bool LeaveGuild(Snowflake guildId)
        {
            return endpoint.LeaveGuild(guildId);
        }

        public DiscordDMChannel[] GetCurrentUserDMs()
        {
            DiscordApiData data = endpoint.GetCurrentUserDMs();
            DiscordDMChannel[] dms = new DiscordDMChannel[data.Values.Count];

            for (int i = 0; i < dms.Length; i++)
                dms[i] = new DiscordDMChannel(data.Values[i]);

            return dms;
        }

        public DiscordConnection[] GetCurrentUserConnections()
        {
            DiscordApiData data = endpoint.GetCurrentUserConnections();
            DiscordConnection[] connections = new DiscordConnection[data.Values.Count];

            for (int i = 0; i < connections.Length; i++)
                connections[i] = new DiscordConnection(data.Values[i]);

            return connections;
        }
    }
}
