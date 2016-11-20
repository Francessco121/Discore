using System.Collections.Generic;
using System.Net.Http;

namespace Discore.Http.Net
{
    class HttpChannelsEndpoint : HttpApiEndpoint
    {
        public HttpChannelsEndpoint(RestClient restClient) 
            : base(restClient)
        { }

        public DiscordApiData Get(Snowflake id)
        {
            return Rest.Get($"channels/{id}", "GetChannel");
        }

        public DiscordApiData Modify(Snowflake channelId, string name = null, int? position = null, 
            string topic = null, int? bitrate = null, int? userLimit = null)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("name", name);
            data.Set("position", position);
            data.Set("topic", topic);
            data.Set("bitrate", bitrate);
            data.Set("user_limit", userLimit);

            return Rest.Patch($"channels/{channelId}", data, "ModifyChannel");
        }

        public DiscordApiData Delete(Snowflake channelId)
        {
            return Rest.Delete($"channels/{channelId}", "DeleteChannel");
        }

        public DiscordApiData GetMessages(Snowflake channelId,
            Snowflake? baseMessageId = null, int? limit = null, DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("limit", limit);
            data.Set(getStrategy.ToString().ToLower(), baseMessageId);

            return Rest.Get($"channels/{channelId}/messages", data, "GetChannelMessages");
        }

        public DiscordApiData GetMessage(Snowflake channelId, Snowflake messageId)
        {
            return Rest.Get($"channels/{channelId}/messages/{messageId}", "GetChannelMessage");
        }

        public DiscordApiData CreateMessage(Snowflake channelId, string content, bool? tts = null, Snowflake? nonce = null)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("content", content);
            data.Set("tts", tts);
            data.Set("nonce", nonce);

            return Rest.Post($"channels/{channelId}/messages", data, "CreateMessage");
        }

        public DiscordApiData CreateReaction(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            return Rest.Put($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me", "CreateReaction");
        }

        public DiscordApiData DeleteOwnReaction(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            return Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me", "DeleteOwnReaction");
        }

        public DiscordApiData DeleteUserReaction(Snowflake channelId, Snowflake messageId, Snowflake userId, DiscordReactionEmoji emoji)
        {
            return Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/{userId}", "DeleteUserReaction");
        }

        public DiscordApiData GetReactions(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            return Rest.Get($"channels/{channelId}/messages/{messageId}/reactions/{emoji}", "GetReactions");
        }

        public DiscordApiData DeleteAllReactions(Snowflake channelId, Snowflake messageId)
        {
            return Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions", "DeleteAllReactions");
        }

        public DiscordApiData UploadFile(Snowflake channelId, byte[] file, 
            string message = null, bool? tts = null, Snowflake? nonce = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                $"{RestClient.BASE_URL}/channels/{channelId}/messages");

            MultipartFormDataContent data = new MultipartFormDataContent();
            data.Add(new ByteArrayContent(file), "file", "file.jpeg");
            request.Content = data;

            if (message != null) request.Properties.Add("content", message);
            if (tts.HasValue)    request.Properties.Add("tts", tts.Value);
            if (nonce.HasValue)  request.Properties.Add("nonce", nonce.Value);

            return Rest.Send(request, "UploadFile");
        }

        public DiscordApiData EditMessage(Snowflake channelId, Snowflake messageId, string content)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("content", content);

            return Rest.Patch($"channels/{channelId}/messages/{messageId}", data, "EditMessage");
        }

        public DiscordApiData DeleteMessage(Snowflake channelId, Snowflake messageId)
        {
            return Rest.Delete($"channels/{channelId}/messages/{messageId}", "DeleteMessage");
        }

        public DiscordApiData BulkDeleteMessages(Snowflake channelId, IEnumerable<Snowflake> messageIds)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            DiscordApiData messages = data.Set("messages", new DiscordApiData(DiscordApiDataType.Array));

            foreach (Snowflake messageId in messageIds)
                messages.Values.Add(new DiscordApiData(messageId));

            return Rest.Post($"channels/{channelId}/messages/bulk-delete", data, "BulkDeleteMessages");
        }

        public DiscordApiData EditPermissions(Snowflake channelId, Snowflake overwriteId,
            DiscordPermission allow, DiscordPermission deny, DiscordOverwriteType type)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("allow", (int)allow);
            data.Set("deny", (int)deny);
            data.Set("type", type.ToString().ToLower());

            return Rest.Put($"channels/{channelId}/permissions/{overwriteId}", data, "EditPermissions");
        }

        public DiscordApiData GetInvites(Snowflake channelId)
        {
            return Rest.Get($"channels/{channelId}/invites", "GetInvites");
        }

        public DiscordApiData CreateInvite(Snowflake channelId,
            int? maxAge = null, int? maxUses = null, bool? temporary = null, bool? unique = null)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("max_age", maxAge);
            data.Set("max_uses", maxUses);
            data.Set("temporary", temporary);
            data.Set("unique", unique);

            return Rest.Post($"channels/{channelId}/invites", data, "CreateInvite");
        }

        public DiscordApiData DeletePermission(Snowflake channelId, Snowflake overwriteId)
        {
            return Rest.Delete($"channels/{channelId}/permissions/{overwriteId}", "DeletePermission");
        }

        public DiscordApiData TriggerTypingIndicator(Snowflake channelId)
        {
            return Rest.Post($"channels/{channelId}/typing", "TriggerTypingIndicator");
        }

        public DiscordApiData GetPinnedMessages(Snowflake channelId)
        {
            return Rest.Get($"channels/{channelId}/pins", "GetPinnedMessages");
        }

        public DiscordApiData AddPinnedChannelMessage(Snowflake channelId, Snowflake messageId)
        {
            return Rest.Put($"channels/{channelId}/pins/{messageId}", "AddPinnedChannelMessage");
        }

        public DiscordApiData DeletePinnedChannelMessage(Snowflake channelId, Snowflake messageId)
        {
            return Rest.Delete($"channels/{channelId}/pins/{messageId}", "DeletePinnedChannelMessage");
        }
    }
}
