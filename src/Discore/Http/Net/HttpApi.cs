namespace Discore.Http.Net
{
    /// <summary>
    /// Internal http api used by both the websocket and http implementation.
    /// </summary>
    class HttpApi
    {
        public HttpGatewayEndpoint Gateway { get; }
        public HttpUsersEndpoint Users { get; }

        public HttpApi(IDiscordAuthenticator authenticator)
        {
            RestClient client = new RestClient(authenticator);

            Gateway = new HttpGatewayEndpoint(client);
            Users = new HttpUsersEndpoint(client);
        }
    }
}
