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

        public DiscordApiData GetMessages(Snowflake channelId, int limit = 50,
            Snowflake? baseMessageId = null, DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before)
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

        #region Reactions
        public DiscordApiData CreateReaction(Snowflake channelId, Snowflake messageId, string emojiName)
        {
            return Rest.Put($"channels/{channelId}/messages/{messageId}/reactions/{emojiName}/@me", "CreateReaction");
        }

        public DiscordApiData CreateReaction(Snowflake channelId, Snowflake messageId, string emojiName, Snowflake emojiId)
        {
            return Rest.Put($"channels/{channelId}/messages/{messageId}/reactions/{emojiName}:{emojiId}/@me", "CreateReaction");
        }

        public DiscordApiData DeleteOwnReaction(Snowflake channelId, Snowflake messageId, string emojiName)
        {
            return Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emojiName}/@me", "DeleteOwnReaction");
        }

        public DiscordApiData DeleteOwnReaction(Snowflake channelId, Snowflake messageId, string emojiName, Snowflake emojiId)
        {
            return Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emojiName}:{emojiId}/@me",
                "DeleteOwnReaction");
        }

        public DiscordApiData DeleteUserReaction(Snowflake channelId, Snowflake messageId, Snowflake userId, string emojiName)
        {
            return Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emojiName}/{userId}", 
                "DeleteUserReaction");
        }

        public DiscordApiData DeleteUserReaction(Snowflake channelId, Snowflake messageId, Snowflake userId, 
            string emojiName, Snowflake emojiId)
        {
            return Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emojiName}:{emojiId}/{userId}",
                "DeleteUserReaction");
        }

        public DiscordApiData GetReactions(Snowflake channelId, Snowflake messageId, string emojiName)
        {
            return Rest.Get($"channels/{channelId}/messages/{messageId}/reactions/{emojiName}", "GetReactions");
        }

        public DiscordApiData GetReactions(Snowflake channelId, Snowflake messageId, string emojiName, Snowflake emojiId)
        {
            return Rest.Get($"channels/{channelId}/messages/{messageId}/reactions/{emojiName}:{emojiId}", "GetReactions");
        }
        #endregion
    }
}
