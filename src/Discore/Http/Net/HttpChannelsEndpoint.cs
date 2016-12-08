using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class HttpChannelsEndpoint : HttpApiEndpoint
    {
        public HttpChannelsEndpoint(RestClient restClient) 
            : base(restClient)
        { }

        public async Task<DiscordApiData> Get(Snowflake id)
        {
            return await Rest.Get($"channels/{id}", "GetChannel");
        }

        public async Task<DiscordApiData> Modify(Snowflake channelId, string name = null, int? position = null, 
            string topic = null, int? bitrate = null, int? userLimit = null)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("name", name);
            data.Set("position", position);
            data.Set("topic", topic);
            data.Set("bitrate", bitrate);
            data.Set("user_limit", userLimit);

            return await Rest.Patch($"channels/{channelId}", data, "ModifyChannel");
        }

        public async Task<DiscordApiData> Delete(Snowflake channelId)
        {
            return await Rest.Delete($"channels/{channelId}", "DeleteChannel");
        }

        public async Task<DiscordApiData> GetMessages(Snowflake channelId,
            Snowflake? baseMessageId = null, int? limit = null, DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before)
        {
            string strat = getStrategy.ToString().ToLower();
            string limitStr = limit.HasValue ? $"&limit={limit.Value}" : "";

            return await Rest.Get($"channels/{channelId}/messages?{strat}={baseMessageId}{limitStr}", "GetChannelMessages");
        }

        public async Task<DiscordApiData> GetMessage(Snowflake channelId, Snowflake messageId)
        {
            return await Rest.Get($"channels/{channelId}/messages/{messageId}", "GetChannelMessage");
        }

        public async Task<DiscordApiData> CreateMessage(Snowflake channelId, string content, bool? tts = null, Snowflake? nonce = null)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("content", content);
            data.Set("tts", tts);
            data.Set("nonce", nonce);

            return await Rest.Post($"channels/{channelId}/messages", data, "CreateMessage");
        }

        public async Task<DiscordApiData> CreateReaction(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            return await Rest.Put($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me", "CreateReaction");
        }

        public async Task<DiscordApiData> DeleteOwnReaction(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            return await Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me", "DeleteOwnReaction");
        }

        public async Task<DiscordApiData> DeleteUserReaction(Snowflake channelId, Snowflake messageId, Snowflake userId, DiscordReactionEmoji emoji)
        {
            return await Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/{userId}", "DeleteUserReaction");
        }

        public async Task<DiscordApiData> GetReactions(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            return await Rest.Get($"channels/{channelId}/messages/{messageId}/reactions/{emoji}", "GetReactions");
        }

        public async Task<DiscordApiData> DeleteAllReactions(Snowflake channelId, Snowflake messageId)
        {
            return await Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions", "DeleteAllReactions");
        }
        
        public async Task<DiscordApiData> UploadFile(Snowflake channelId, byte[] file, string filename = "unknown.jpg",
            string message = null, bool? tts = null, Snowflake? nonce = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                $"{RestClient.BASE_URL}/channels/{channelId}/messages");

            MultipartFormDataContent data = new MultipartFormDataContent();
            data.Add(new ByteArrayContent(file), "file", filename);
            request.Content = data;

            if (message != null) request.Properties.Add("content", message);
            if (tts.HasValue)    request.Properties.Add("tts", tts.Value);
            if (nonce.HasValue)  request.Properties.Add("nonce", nonce.Value);

            return await Rest.Send(request, "UploadFile");
        }

        public async Task<DiscordApiData> EditMessage(Snowflake channelId, Snowflake messageId, string content)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("content", content);

            return await Rest.Patch($"channels/{channelId}/messages/{messageId}", data, "EditMessage");
        }

        public async Task<DiscordApiData> DeleteMessage(Snowflake channelId, Snowflake messageId)
        {
            return await Rest.Delete($"channels/{channelId}/messages/{messageId}", "DeleteMessage");
        }

        public async Task<DiscordApiData> BulkDeleteMessages(Snowflake channelId, IEnumerable<Snowflake> messageIds)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            DiscordApiData messages = data.Set("messages", new DiscordApiData(DiscordApiDataType.Array));

            foreach (Snowflake messageId in messageIds)
                messages.Values.Add(new DiscordApiData(messageId));

            return await Rest.Post($"channels/{channelId}/messages/bulk-delete", data, "BulkDeleteMessages");
        }

        public async Task<DiscordApiData> EditPermissions(Snowflake channelId, Snowflake overwriteId,
            DiscordPermission allow, DiscordPermission deny, DiscordOverwriteType type)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("allow", (int)allow);
            data.Set("deny", (int)deny);
            data.Set("type", type.ToString().ToLower());

            return await Rest.Put($"channels/{channelId}/permissions/{overwriteId}", data, "EditPermissions");
        }

        public async Task<DiscordApiData> GetInvites(Snowflake channelId)
        {
            return await Rest.Get($"channels/{channelId}/invites", "GetInvites");
        }

        public async Task<DiscordApiData> CreateInvite(Snowflake channelId,
            int? maxAge = null, int? maxUses = null, bool? temporary = null, bool? unique = null)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            if (maxAge.HasValue) data.Set("max_age", maxAge.Value);
            if (maxUses.HasValue) data.Set("max_uses", maxUses.Value);
            if (temporary.HasValue) data.Set("temporary", temporary.Value);
            if (unique.HasValue) data.Set("unique", unique.Value);

            return await Rest.Post($"channels/{channelId}/invites", data, "CreateInvite");
        }

        public async Task<DiscordApiData> DeletePermission(Snowflake channelId, Snowflake overwriteId)
        {
            return await Rest.Delete($"channels/{channelId}/permissions/{overwriteId}", "DeletePermission");
        }

        public async Task<DiscordApiData> TriggerTypingIndicator(Snowflake channelId)
        {
            return await Rest.Post($"channels/{channelId}/typing", "TriggerTypingIndicator");
        }

        public async Task<DiscordApiData> GetPinnedMessages(Snowflake channelId)
        {
            return await Rest.Get($"channels/{channelId}/pins", "GetPinnedMessages");
        }

        public async Task<DiscordApiData> AddPinnedChannelMessage(Snowflake channelId, Snowflake messageId)
        {
            return await Rest.Put($"channels/{channelId}/pins/{messageId}", "AddPinnedChannelMessage");
        }

        public async Task<DiscordApiData> DeletePinnedChannelMessage(Snowflake channelId, Snowflake messageId)
        {
            return await Rest.Delete($"channels/{channelId}/pins/{messageId}", "DeletePinnedChannelMessage");
        }
    }
}
