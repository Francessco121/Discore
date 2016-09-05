using Discore.Net;

namespace Discore
{
    /// <summary>
    /// A interface to the Discord API.
    /// </summary>
    public interface IDiscordClient : ICacheContainer
    {
        /// <summary>
        /// Gets a rest client interface to the Discord REST API.
        /// </summary>
        IDiscordRestClient Rest { get; }
        /// <summary>
        /// Gets an interface to the Discord gateway.
        /// </summary>
        IDiscordGateway Gateway { get; }
        /// <summary>
        /// Gets the user authenticated with this client.
        /// </summary>
        DiscordUser User { get; }
    }
}
