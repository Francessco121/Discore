namespace Discore.WebSocket
{
    public interface ITextChannel
    {
        DiscordMessage SendMessage(string content);
    }
}
