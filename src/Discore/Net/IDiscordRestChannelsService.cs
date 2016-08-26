using System.Threading.Tasks;

namespace Discore.Net
{
    public interface IDiscordRestChannelsService
    {
        Task<DiscordChannel> Get(string channelId, DiscordChannelType type);
        Task<DiscordGuildChannel> Modify(DiscordGuildChannel guildChannel, DiscordGuildChannelModifyParams settings);
        Task<DiscordChannel> Close(DiscordChannel channel);
    }
}
