using System;
using System.Threading.Tasks;

namespace Discore.Net
{
    class RestClientUsersService : RestClientService, IDiscordRestUsersService
    {
        public RestClientUsersService(DiscordClient client, RestClient rest) 
            : base(client, rest)
        { }

        public async Task<DiscordDMChannel> CreateDM(string recipientId)
        {
            if (recipientId == null)
                throw new ArgumentNullException("recipientId");
            if (recipientId == client.User.Id)
                throw new InvalidOperationException("Cannot open a DM channel with self!");

            DiscordApiData data = new DiscordApiData();
            data.Set("recipient_id", recipientId);

            DiscordApiData response = await Post($"users/{client.User.Id}/channels", data, "CreateDM");
            return response.IsNull ? null : (DiscordDMChannel)cacheHelper.CreateChannel(response);
        }
    }
}
