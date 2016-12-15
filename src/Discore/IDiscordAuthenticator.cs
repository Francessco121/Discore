namespace Discore
{
    /// <summary>
    /// Represents an authentication method for the Discord API.
    /// </summary>
    public interface IDiscordAuthenticator
    {
        /// <summary>
        /// Gets whether this authenticator can be used to authenticate with the WebSocket API.
        /// </summary>
        bool CanAuthenticateWebSocket { get; }

        /// <summary>
        /// Gets the token used by this authenticator.
        /// </summary>
        string GetToken();
        /// <summary>
        /// Gets the HTTP auth type used by this authenticator.
        /// </summary>
        string GetTokenHttpType();
    }
}
