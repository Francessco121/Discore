using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discore.Net
{
    class RestClientMessagesService : RestClientService, IDiscordRestMessagesService
    {
        public RestClientMessagesService(DiscordClient client, RestClient rest) 
            : base(client, rest)
        { }

        public async Task<DiscordMessage> Send(DiscordChannel channel, string message)
        {
            if (channel.ChannelType == DiscordChannelType.Guild)
            {
                DiscordGuildChannel guildChannel = (DiscordGuildChannel)channel;
                if (guildChannel.GuildChannelType == DiscordGuildChannelType.Voice)
                    throw new ArgumentException("Cannot send message in a voice channel", "channel");

                client.User.AssertPermission(DiscordPermission.SendMessages, guildChannel);
            }

            DiscordApiData data = new DiscordApiData();
            data.Set("content", message);

            DiscordApiData response = await Post($"channels/{channel.Id}/messages", data, "SendMessage");
            return response.IsNull ? null : cacheHelper.CreateMessage(response);
        }

        public async Task<DiscordMessage> Send(DiscordChannel channel, string message, byte[] file)
        {
            if (channel.ChannelType == DiscordChannelType.Guild)
            {
                DiscordGuildChannel guildChannel = (DiscordGuildChannel)channel;
                if (guildChannel.GuildChannelType == DiscordGuildChannelType.Voice)
                    throw new ArgumentException("Cannot send message in a voice channel", "channel");

                client.User.AssertPermission(DiscordPermission.SendMessages | DiscordPermission.AttachFiles, guildChannel);
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, 
                $"{RestClient.BASE_URL}/channels/{channel.Id}/messages");
            MultipartFormDataContent data = new MultipartFormDataContent();
            data.Add(new ByteArrayContent(file), "file", "file.jpeg");
            request.Content = data;
            request.Properties.Add("content", message);

            DiscordApiData response = await Post(request, "SendMessageWithAttachment");
            return response.IsNull ? null : cacheHelper.CreateMessage(response);
        }

        public async Task<DiscordMessage> Get(DiscordChannel channel, string messageId)
        {
            if (channel.ChannelType == DiscordChannelType.Guild)
            {
                DiscordGuildChannel guildChannel = (DiscordGuildChannel)channel;
                if (guildChannel.GuildChannelType == DiscordGuildChannelType.Voice)
                    throw new ArgumentException("Cannot get a message from a voice channel", "channel");

                client.User.AssertPermission(DiscordPermission.ReadMessageHistory, guildChannel);
            }

            DiscordApiData data = await Get($"channels/{channel.Id}/messages/{messageId}", "GetMessage");

            return data.IsNull ? null : cacheHelper.CreateMessage(data);
        }

        public async Task<DiscordMessage[]> Get(DiscordChannel channel, DiscordMessageGetStrategy strategy,
            string baseMessageId, int limit = 50)
        {
            if (limit < 1 || limit > 100)
                throw new ArgumentOutOfRangeException("limit", "Message limit must be between 1 and 100");

            if (channel.ChannelType == DiscordChannelType.Guild)
            {
                DiscordGuildChannel guildChannel = (DiscordGuildChannel)channel;
                if (guildChannel.GuildChannelType == DiscordGuildChannelType.Voice)
                    throw new ArgumentException("Cannot get messages from a voice channel", "channel");

                client.User.AssertPermission(DiscordPermission.ReadMessageHistory, guildChannel);
            }

            string strategyStr = strategy.ToString().ToLower();

            DiscordApiData data = await Get($"channels/{channel.Id}/messages?limit={limit}&{strategyStr}={baseMessageId}",
                "GetMessages");

            if (!data.IsNull)
            {
                DiscordMessage[] messages = new DiscordMessage[data.Values.Count];
                for (int i = 0; i < data.Values.Count; i++)
                {
                    DiscordMessage msg = cacheHelper.CreateMessage(data.Values[i]);
                    messages[i] = msg;
                }

                return messages;
            }
            else
                return null;
        }

        public async Task<DiscordMessage> Edit(DiscordChannel channel, string messageId, string content)
        {
            if (channel.ChannelType == DiscordChannelType.Guild)
            {
                DiscordGuildChannel guildChannel = (DiscordGuildChannel)channel;
                if (guildChannel.GuildChannelType == DiscordGuildChannelType.Voice)
                    throw new ArgumentException("Cannot edit messages in a voice channel", "channel");

                client.User.AssertPermission(DiscordPermission.ReadMessageHistory, guildChannel);
            }

            DiscordApiData requestData = new DiscordApiData();
            requestData.Set("content", content);

            DiscordApiData responseData = await Patch($"channels/{channel.Id}/messages/{messageId}", requestData, "EditMessage");
            if (!responseData.IsNull)
            {
                DiscordMessage msg = new DiscordMessage(client);
                msg.Update(responseData);

                return msg;
            }
            else
                return null;
        }

        public async Task<bool> Delete(DiscordChannel channel, string messageId)
        {
            if (channel.ChannelType == DiscordChannelType.Guild)
            {
                DiscordGuildChannel guildChannel = (DiscordGuildChannel)channel;
                if (guildChannel.GuildChannelType == DiscordGuildChannelType.Voice)
                    throw new ArgumentException("Cannot delete a message in a voice channel", "channel");

                client.User.AssertPermission(DiscordPermission.ManageMessages, guildChannel);
            }

            DiscordApiData data = await Delete($"channels/{channel.Id}/messages/{messageId}", "DeleteMessage");
            DiscordMessage message;
            channel.TryRemoveMessage(messageId, out message);
            return data.Value == null;
        }

        public async Task<bool> Delete(DiscordChannel channel, string[] messageIds)
        {
            if (messageIds.Length == 0)
                return false;
            else if (messageIds.Length == 1)
            {
                return await Delete(channel, messageIds[0]);
            }
            else
            {
                if (channel.ChannelType == DiscordChannelType.Guild)
                {
                    DiscordGuildChannel guildChannel = (DiscordGuildChannel)channel;
                    if (guildChannel.GuildChannelType == DiscordGuildChannelType.Voice)
                        throw new ArgumentException("Cannot delete messages in a voice channel", "channel");

                    client.User.AssertPermission(DiscordPermission.ManageMessages, guildChannel);
                }

                DiscordApiData deleteData = new DiscordApiData();
                deleteData.Set("messages", messageIds);

                for (int i = 0; i < messageIds.Length; i++)
                {
                    DiscordMessage msg;
                    channel.TryRemoveMessage(messageIds[i], out msg);
                }

                DiscordApiData data = await Post($"channels/{channel.Id}/messages/bulk_delete", deleteData, "DeleteMessages");
                return data.Value == null;
            }
        }

        public async Task<bool> Pin(DiscordMessage message)
        {
            DiscordApiData data = await Put($"channels/{message.Channel.Id}/pins/{message.Id}", null, "PinMessage");
            return data.Value == null;
        }
    }
}
