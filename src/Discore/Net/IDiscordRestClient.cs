namespace Discore.Net
{
    public interface IDiscordRestClient
    {
        IDiscordRestMessagesService Messages { get; }
        IDiscordRestChannelsService Channels { get; }
    }
}
