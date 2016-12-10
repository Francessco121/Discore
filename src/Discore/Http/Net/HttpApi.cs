namespace Discore.Http.Net
{
    /// <summary>
    /// Internal http api used by both the websocket and http implementation.
    /// </summary>
    class HttpApi
    {
        public DiscordHttpWebhookEndpoint Webhooks { get; }
        public DiscordHttpVoiceEndpoint Voice { get; }

        public HttpApi(IDiscordAuthenticator authenticator)
        {
            RestClient client = new RestClient(authenticator);

            Webhooks = new DiscordHttpWebhookEndpoint(client);
            Voice = new DiscordHttpVoiceEndpoint(client);
        }
    }
}
