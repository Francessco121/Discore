using System.Threading.Tasks;

namespace Discore.Net
{
    public interface IDiscordRestMessagesService
    {
        Task<DiscordMessage> Send(DiscordChannel channel, string message);
        Task<DiscordMessage> Send(DiscordChannel channel, string message, byte[] file);
        Task<DiscordMessage> Get(DiscordChannel channel, string channelId);
        Task<DiscordMessage[]> Get(DiscordChannel channel, DiscordMessageGetStrategy strategy, string baseMessageId, int limit = 50);
        Task<DiscordMessage> Edit(DiscordChannel channel, string messageId, string content);
        Task<bool> Delete(DiscordChannel channel, string messageId);
        Task<bool> Delete(DiscordChannel channel, string[] messageIds);
    }
}
