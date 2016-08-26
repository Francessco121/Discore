namespace Discore.Net
{
    /// <summary>
    /// Provides interaction with the Discord REST API.
    /// </summary>
    public interface IDiscordRestClient
    {
        /// <summary>
        /// Gets the service used for interacting with <see cref="DiscordMessage"/>s.
        /// </summary>
        IDiscordRestMessagesService Messages { get; }

        /// <summary>
        /// Gets the service used for interacting with <see cref="DiscordChannel"/>s.
        /// </summary>
        IDiscordRestChannelsService Channels { get; }
    }
}
