using System.Threading.Tasks;

namespace Discore.Net
{
    class RestClientChannelsService : RestClientService, IDiscordRestChannelsService
    {
        public RestClientChannelsService(DiscordClient client, RestClient rest) 
            : base(client, rest)
        { }

        public async Task<DiscordChannel> Get(string channelId, DiscordChannelType type)
        {
            DiscordApiData response = await Get($"channels/{channelId}", "GetChannel");

            return client.CacheHelper.UpdateChannel(response);
        }

        public async Task<DiscordGuildChannel> Modify(DiscordGuildChannel guildChannel,
            DiscordGuildChannelModifyParams settings)
        {
            client.User.AssertPermission(DiscordPermission.ManageGuild, guildChannel);

            DiscordApiData requestData = new DiscordApiData();
            requestData.Set("name", settings.Name);
            requestData.Set("position", settings.Position);

            if (settings.Type == DiscordGuildChannelType.Text)
            {
                requestData.Set("topic", settings.Topic);
            }
            else if (settings.Type == DiscordGuildChannelType.Voice)
            {
                requestData.Set("bitrate", settings.Bitrate);
                requestData.Set("user_limit", settings.UserLimit);
            }

            DiscordApiData responseData = await Put($"channels/{guildChannel.Id}", requestData, "ModifyChannel");
            DiscordGuildChannel channel = cacheHelper.UpdateChannel(responseData);

            return channel;
        }

        public async Task<DiscordChannel> Close(DiscordChannel channel)
        {
            DiscordGuildChannel guildChannel = channel as DiscordGuildChannel;
            if (guildChannel != null)
                client.User.AssertPermission(DiscordPermission.ManageGuild, guildChannel.Guild);

            await Delete($"channels/{channel.Id}", "DeleteChannel");
            return channel;
        }
    }
}
