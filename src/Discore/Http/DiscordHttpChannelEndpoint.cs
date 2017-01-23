using Discore.Http.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Discore.Http
{
    public sealed class DiscordHttpChannelEndpoint : DiscordHttpApiEndpoint
    {
        internal DiscordHttpChannelEndpoint(IDiscordApplication app, RestClient rest)
            : base(app, rest)
        { }

        #region Channel Management
        /// <summary>
        /// Gets a DM or guild channel by ID.
        /// </summary>
        public Task<DiscordChannel> Get(Snowflake channelId)
        {
            return Get<DiscordChannel>(channelId);
        }

        /// <summary>
        /// Gets a DM or guild channel by ID.
        /// </summary>
        public async Task<T> Get<T>(Snowflake channelId) 
            where T : DiscordChannel
        {
            DiscordApiData data = await Rest.Get($"channels/{channelId}", "GetChannel").ConfigureAwait(false);
            return (T)DeserializeChannelData(data);
        }

        /// <summary>
        /// Updates the settings of a text guild channel.
        /// </summary>
        /// <param name="channelId">The id of the guild channel to modify.</param>
        /// <param name="name">The name of the channel (or null to leave unchanged).</param>
        /// <param name="position">The UI position of the channel (or null to leave unchanged).</param>
        /// <param name="topic">The topic of the text channel (or null to leave unchanged).</param>
        public Task<DiscordGuildTextChannel> ModifyTextChannel(Snowflake channelId,
            string name = null, int? position = null, string topic = null)
        {
            return Modify<DiscordGuildTextChannel>(channelId, name, position, topic);
        }

        /// <summary>
        /// Updates the settings of a voice guild channel.
        /// </summary>
        /// <param name="channelId">The id of the guild channel to modify.</param>
        /// <param name="name">The name of the channel (or null to leave unchanged).</param>
        /// <param name="position">The UI position of the channel (or null to leave unchanged).</param>
        /// <param name="bitrate">The bitrate of the voice channel (or null to leave unchanged).</param>
        /// <param name="userLimit">The user limit of the voice channel (or null to leave unchanged).</param>
        public Task<DiscordGuildVoiceChannel> ModifyVoiceChannel(Snowflake channelId,
            string name = null, int? position = null, int? bitrate = null, int? userLimit = null)
        {
            return Modify<DiscordGuildVoiceChannel>(channelId, name, position, null, bitrate, userLimit);
        }

        /// <summary>
        /// Updates the settings of a text or voice guild channel.
        /// </summary>
        /// <param name="channelId">The id of the guild channel to modify.</param>
        /// <param name="name">The name of the channel (or null to leave unchanged).</param>
        /// <param name="position">The UI position of the channel (or null to leave unchanged).</param>
        /// <param name="topic">The topic of the text channel (or null to leave unchanged).</param>
        /// <param name="bitrate">The bitrate of the voice channel (or null to leave unchanged).</param>
        /// <param name="userLimit">The user limit of the voice channel (or null to leave unchanged).</param>
        public async Task<T> Modify<T>(Snowflake channelId,
            string name = null, int? position = null, 
            string topic = null,
            int? bitrate = null, int? userLimit = null)
            where T : DiscordGuildChannel
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("name", name);
            requestData.Set("position", position);
            requestData.Set("topic", topic);
            requestData.Set("bitrate", bitrate);
            requestData.Set("user_limit", userLimit);

            DiscordApiData returnData = await Rest.Patch($"channels/{channelId}", requestData, "ModifyChannel").ConfigureAwait(false);
            return (T)DeserializeChannelData(returnData);            
        }

        /// <summary>
        /// Deletes a guild channel, or closes a DM.
        /// </summary>
        public Task<DiscordChannel> Delete(Snowflake channelId)
        {
            return Delete<DiscordChannel>(channelId);
        }

        /// <summary>
        /// Deletes a guild channel, or closes a DM.
        /// </summary>
        public async Task<T> Delete<T>(Snowflake channelId)
            where T : DiscordChannel
        {
            DiscordApiData data = await Rest.Delete($"channels/{channelId}", "DeleteChannel").ConfigureAwait(false);
            return (T)DeserializeChannelData(data);
        }
        #endregion

        #region Permission Management
        /// <summary>
        /// Edits a guild channel permission overwrite for a user or role.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> EditPermissions(Snowflake channelId, Snowflake overwriteId,
            DiscordPermission allow, DiscordPermission deny, DiscordOverwriteType type)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("allow", (int)allow);
            data.Set("deny", (int)deny);
            data.Set("type", type.ToString().ToLower());

            return (await Rest.Put($"channels/{channelId}/permissions/{overwriteId}", data, "EditChannelPermissions").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Deletes a guild channel permission overwrite for a user or role.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> DeletePermission(Snowflake channelId, Snowflake overwriteId)
        {
            return (await Rest.Delete($"channels/{channelId}/permissions/{overwriteId}", "DeleteChannelPermission").ConfigureAwait(false)).IsNull;
        }
        #endregion

        #region Message Management
        /// <summary>
        /// Gets messages from a text channel.
        /// </summary>
        public async Task<IReadOnlyList<DiscordMessage>> GetMessages(Snowflake channelId,
            Snowflake? baseMessageId = null, int? limit = null,
            DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before)
        {
            string strat = getStrategy.ToString().ToLower();
            string limitStr = limit.HasValue ? $"&limit={limit.Value}" : "";

            DiscordApiData data = await Rest.Get($"channels/{channelId}/messages?{strat}={baseMessageId}{limitStr}", "GetChannelMessages").ConfigureAwait(false);
            DiscordMessage[] messages = new DiscordMessage[data.Values.Count];

            for (int i = 0; i < messages.Length; i++)
                messages[i] = new DiscordMessage(App, data.Values[i]);

            return messages;
        }

        /// <summary>
        /// Gets a single message by ID from a channel.
        /// </summary>
        public async Task<DiscordMessage> GetMessage(Snowflake channelId, Snowflake messageId)
        {
            DiscordApiData data = await Rest.Get($"channels/{channelId}/messages/{messageId}", "GetChannelMessage").ConfigureAwait(false);
            return new DiscordMessage(App, data);
        }

        /// <summary>
        /// Posts a message to a text channel.
        /// </summary>
        public async Task<DiscordMessage> CreateMessage(Snowflake channelId, string content, bool tts = false, Snowflake? nonce = null)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("content", content);
            requestData.Set("tts", tts);
            requestData.Set("nonce", nonce);

            DiscordApiData returnData = await Rest.Post($"channels/{channelId}/messages", requestData, "CreateMessage").ConfigureAwait(false);
            return new DiscordMessage(App, returnData);
        }

        /// <summary>
        /// Uploads a file to a text channel with an optional message.
        /// </summary>
        public async Task<DiscordMessage> UploadFile(Snowflake channelId, FileInfo fileInfo, 
            string message = null, bool tts = false, Snowflake? nonce = null)
        {
            using (FileStream fs = fileInfo.OpenRead())
            using (MemoryStream ms = new MemoryStream())
            {
                await fs.CopyToAsync(ms).ConfigureAwait(false);
                return await UploadFile(channelId, ms.ToArray(), fileInfo.Name, message, tts, nonce).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Uploads a file to a text channel with an optional message.
        /// </summary>
        public async Task<DiscordMessage> UploadFile(Snowflake channelId, byte[] file, string filename = "unknown.jpg",
            string message = null, bool? tts = null, Snowflake? nonce = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                $"{RestClient.BASE_URL}/channels/{channelId}/messages");

            MultipartFormDataContent data = new MultipartFormDataContent();
            data.Add(new ByteArrayContent(file), "file", filename);
            request.Content = data;

            if (message != null) request.Properties.Add("content", message);
            if (tts.HasValue) request.Properties.Add("tts", tts.Value);
            if (nonce.HasValue) request.Properties.Add("nonce", nonce.Value);

            DiscordApiData returnData = await Rest.Send(request, "UploadFile").ConfigureAwait(false);
            return new DiscordMessage(App, returnData);
        }

        /// <summary>
        /// Edits an existing message in a text channel.
        /// </summary>
        public async Task<DiscordMessage> EditMessage(Snowflake channelId, Snowflake messageId, string content)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("content", content);

            DiscordApiData returnData = await Rest.Patch($"channels/{channelId}/messages/{messageId}", requestData, "EditMessage").ConfigureAwait(false);
            return new DiscordMessage(App, returnData);
        }

        /// <summary>
        /// Deletes a message from a text channel.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> DeleteMessage(Snowflake channelId, Snowflake messageId)
        {
            DiscordApiData data = await Rest.Delete($"channels/{channelId}/messages/{messageId}", "DeleteMessage").ConfigureAwait(false);
            return data.IsNull;
        }

        /// <summary>
        /// Deletes a group of messages all at once from a text channel.
        /// This is much faster than calling DeleteMessage for each message.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> BulkDeleteMessages(Snowflake channelId, IEnumerable<DiscordMessage> messages)
        {
            List<Snowflake> msgIds = new List<Snowflake>();
            foreach (DiscordMessage msg in messages)
                msgIds.Add(msg.Id);

            return await BulkDeleteMessages(channelId, msgIds).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes a group of messages all at once from a text channel.
        /// This is much faster than calling DeleteMessage for each message.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> BulkDeleteMessages(Snowflake channelId, IEnumerable<Snowflake> messageIds)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            DiscordApiData messages = requestData.Set("messages", new DiscordApiData(DiscordApiDataType.Array));

            foreach (Snowflake messageId in messageIds)
                messages.Values.Add(new DiscordApiData(messageId));

            DiscordApiData returnData = await Rest.Post($"channels/{channelId}/messages/bulk-delete", requestData, "BulkDeleteMessages").ConfigureAwait(false);
            return returnData.IsNull;
        }

        /// <summary>
        /// Gets a list of all pinned messages in a text channel.
        /// </summary>
        public async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessages(Snowflake channelId)
        {
            DiscordApiData data = await Rest.Get($"channels/{channelId}/pins", "GetPinnedChannelMessages").ConfigureAwait(false);
            DiscordMessage[] messages = new DiscordMessage[data.Values.Count];

            for (int i = 0; i < messages.Length; i++)
                messages[i] = new DiscordMessage(App, data.Values[i]);

            return messages;
        }

        /// <summary>
        /// Pins a message in a text channel.
        /// </summary>
        public async Task<bool> AddPinnedMessage(Snowflake channelId, Snowflake messageId)
        {
            return (await Rest.Put($"channels/{channelId}/pins/{messageId}", "AddPinnedChannelMessage").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Unpins a message from a text channel.
        /// </summary>
        public async Task<bool> DeletePinnedMessage(Snowflake channelId, Snowflake messageId)
        {
            return (await Rest.Delete($"channels/{channelId}/pins/{messageId}", "DeletePinnedChannelMessage").ConfigureAwait(false)).IsNull;
        }
        #endregion

        #region Reaction Management
        /// <summary>
        /// Adds a reaction to a message.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> CreateReaction(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            return (await Rest.Put($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me", "CreateReaction").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Deletes a reaction the currently authenticated user has made for a message.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> DeleteOwnReaction(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            return (await Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me", "DeleteOwnReaction").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Deletes a reaction posted by any user.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> DeleteUserReaction(Snowflake channelId, Snowflake messageId, Snowflake userId, DiscordReactionEmoji emoji)
        {
            return (await Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/{userId}", "DeleteUserReaction").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Gets a list of all users who reacted to the specified message with the specified emoji.
        /// </summary>
        public async Task<IReadOnlyList<DiscordUser>> GetReactions(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            DiscordApiData data = await Rest.Get($"channels/{channelId}/messages/{messageId}/reactions/{emoji}", "GetReactions").ConfigureAwait(false);

            DiscordUser[] users = new DiscordUser[data.Values.Count];
            for (int i = 0; i < users.Length; i++)
                users[i] = new DiscordUser(data.Values[i]);

            return users;
        }

        /// <summary>
        /// Deletes all reactions on a message.
        /// </summary>
        public async Task DeleteAllReactions(Snowflake channelId, Snowflake messageId)
        {
            await Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions", "DeleteAllReactions").ConfigureAwait(false);
        }
        #endregion

        #region Invite Management
        /// <summary>
        /// Gets a list of all invites for the specified guild channel.
        /// </summary>
        public async Task<IReadOnlyList<DiscordInviteMetadata>> GetInvites(Snowflake channelId)
        {
            DiscordApiData data = await Rest.Get($"channels/{channelId}/invites", "GetChannelInvites").ConfigureAwait(false);

            DiscordInviteMetadata[] invites = new DiscordInviteMetadata[data.Values.Count];
            for (int i = 0; i < invites.Length; i++)
                invites[i] = new DiscordInviteMetadata(App, data.Values[i]);

            return invites;
        }

        /// <summary>
        /// Creates a new invite for the specified guild channel.
        /// </summary>
        /// <param name="channelId">The ID of the guild channel.</param>
        /// <param name="maxAge">The duration of invite before expiry, or 0 or null for never.</param>
        /// <param name="maxUses">The max number of uses or 0 or null for unlimited.</param>
        /// <param name="temporary">Whether this invite only grants temporary membership.</param>
        /// <param name="unique">
        /// If true, don't try to reuse a similar invite 
        /// (useful for creating many unique one time use invites).
        /// </param>
        public async Task<DiscordInvite> CreateInvite(Snowflake channelId,
            TimeSpan? maxAge = null, int? maxUses = null, bool? temporary = null, bool? unique = null)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            if (maxAge.HasValue) requestData.Set("max_age", maxAge.Value);
            if (maxUses.HasValue) requestData.Set("max_uses", maxUses.Value);
            if (temporary.HasValue) requestData.Set("temporary", temporary.Value);
            if (unique.HasValue) requestData.Set("unique", unique.Value);

            DiscordApiData returnData = await Rest.Post($"channels/{channelId}/invites", requestData, "CreateChannelInvite").ConfigureAwait(false);
            return new DiscordInvite(App, returnData);
        }
        #endregion

        /// <summary>
        /// Causes the current authenticated user to appear as typing in this channel.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> TriggerTypingIndicator(Snowflake channelId)
        {
            return (await Rest.Post($"channels/{channelId}/typing", "TriggerTypingIndicator").ConfigureAwait(false)).IsNull;
        }
    }
}
