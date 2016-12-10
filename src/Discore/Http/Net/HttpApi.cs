namespace Discore.Http.Net
{
    /// <summary>
    /// Internal http api used by both the websocket and http implementation.
    /// </summary>
    class HttpApi
    {
        public HttpGatewayEndpoint Gateway { get; }
        public HttpWebhookEndpoint Webhooks { get; }
        public HttpVoiceEndpoint Voice { get; }

        public HttpApi(IDiscordAuthenticator authenticator)
        {
            RestClient client = new RestClient(authenticator);

            Gateway = new HttpGatewayEndpoint(client);
            Webhooks = new HttpWebhookEndpoint(client);
            Voice = new HttpVoiceEndpoint(client);
        }
    }
}
