namespace Discore.Http.Net
{
    public abstract class DiscordHttpApiEndpoint
    {
        internal IDiscordApplication App { get; }
        internal RestClient Rest { get; }

        internal DiscordHttpApiEndpoint(IDiscordApplication app, RestClient rest)
        {
            App = app;
            Rest = rest;
        }
    }
}
