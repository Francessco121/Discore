namespace Discore.Http.Net
{
    class HttpUsersEndpoint : HttpApiEndpoint
    {
        public HttpUsersEndpoint(RestClient restClient) 
            : base(restClient)
        { }

        public DiscordApiData GetCurrentUser()
        {
            return Rest.Get("users/@me", "GetCurrentUser");
        }

        public DiscordApiData Get(Snowflake id)
        {
            return Rest.Get($"users/{id}", "GetUser");
        }

        public DiscordApiData ModifyCurrentUser(string username = null, DiscordAvatarData avatar = null)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("username", username);
            data.Set("avatar", avatar.ToFormattedString());

            return Rest.Patch("users/@me", data, "ModifyCurrentUser");
        }

        public DiscordApiData GetCurrentUserGuilds()
        {
            return Rest.Get($"users/@me/guilds", "GetCurrentUserGuilds");
        }

        public bool LeaveGuild(Snowflake guildId)
        {
            return Rest.Delete($"users/@me/guilds/{guildId}", "LeaveGuild").IsNull;
        }

        public DiscordApiData GetCurrentUserDMs()
        {
            return Rest.Get("users/@me/channels", "GetCurrentUserDMs");
        }

        public DiscordApiData CreateDM(Snowflake recipientId)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("recipient_id", recipientId);

            return Rest.Post("users/@me/channels", data, "CreateDM");
        }

        public DiscordApiData GetCurrentUserConnections()
        {
            return Rest.Get("users/@me/connections", "GetCurrentUserConnections");
        }
    }
}
