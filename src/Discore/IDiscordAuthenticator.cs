namespace Discore
{
    public interface IDiscordAuthenticator
    {
        bool CanAuthenticateWebSocket { get; }

        string GetToken();
        string GetTokenHttpType();
    }
}
