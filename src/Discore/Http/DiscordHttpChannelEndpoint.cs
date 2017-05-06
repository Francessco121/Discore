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
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordChannel> Get(Snowflake channelId)
        {
            return Get<DiscordChannel>(channelId);
        }

        /// <summary>
        /// Gets a DM or guild channel by ID.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<T> Get<T>(Snowflake channelId) 
            where T : DiscordChannel
        {
            DiscordApiData data = await Rest.Get($"channels/{channelId}", "channels/channel").ConfigureAwait(false);
            return (T)DeserializeChannelData(data);
        }

        #region Deprecated Modify* Methods
        /// <summary>
        /// Updates the settings of a text guild channel.
        /// </summary>
        /// <param name="channelId">The id of the guild channel to modify.</param>
        /// <param name="name">The name of the channel (or null to leave unchanged).</param>
        /// <param name="position">The UI position of the channel (or null to leave unchanged).</param>
        /// <param name="topic">The topic of the text channel (or null to leave unchanged).</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        [Obsolete("Please use the ModifyTextChannel overload using a builder object instead.")]
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
        /// <exception cref="DiscordHttpApiException"></exception>
        [Obsolete("Please use the ModifyVoiceChannel overload using a builder object instead.")]
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
        /// <exception cref="DiscordHttpApiException"></exception>
        [Obsolete("Please use either ModifyTextChannel() or ModifyVoiceChannel() instead.")]
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

            DiscordApiData returnData = await Rest.Patch($"channels/{channelId}", requestData, 
                "channels/channel").ConfigureAwait(false);
            return (T)DeserializeChannelData(returnData);            
        }
        #endregion

        /// <summary>
        /// Updates the settings of a guild text channel.
        /// </summary>
        /// <param name="textChannelId">The ID of the guild text channel to modify.</param>
        /// <param name="parameters">A set of parameters to modify the channel with.</param>
        /// <returns>Returns the updated guild text channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameters"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildTextChannel> ModifyTextChannel(Snowflake textChannelId, 
            GuildTextChannelParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Patch($"channels/{textChannelId}", requestData,
                "channels/channel").ConfigureAwait(false);
            return (DiscordGuildTextChannel)DeserializeChannelData(returnData);
        }

        /// <summary>
        /// Updates the settings of a guild voice channel.
        /// </summary>
        /// <param name="voiceChannelId">The ID of the guild voice channel to modify.</param>
        /// <param name="parameters">A set of parameters to modify the channel with.</param>
        /// <returns>Returns the updated guild voice channel.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameters"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordGuildVoiceChannel> ModifyVoiceChannel(Snowflake voiceChannelId, 
            GuildVoiceChannelParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            DiscordApiData requestData = parameters.Build();

            DiscordApiData returnData = await Rest.Patch($"channels/{voiceChannelId}", requestData,
                "channels/channel").ConfigureAwait(false);
            return (DiscordGuildVoiceChannel)DeserializeChannelData(returnData);
        }

        /// <summary>
        /// Deletes a guild channel, or closes a DM.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordChannel> Delete(Snowflake channelId)
        {
            return Delete<DiscordChannel>(channelId);
        }

        /// <summary>
        /// Deletes a guild channel, or closes a DM.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<T> Delete<T>(Snowflake channelId)
            where T : DiscordChannel
        {
            DiscordApiData data = await Rest.Delete($"channels/{channelId}", "channels/channel").ConfigureAwait(false);
            return (T)DeserializeChannelData(data);
        }
        #endregion

        #region Permission Management
        /// <summary>
        /// Edits a guild channel permission overwrite for a user or role.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> EditPermissions(Snowflake channelId, Snowflake overwriteId,
            DiscordPermission allow, DiscordPermission deny, DiscordOverwriteType type)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("allow", (int)allow);
            data.Set("deny", (int)deny);
            data.Set("type", type.ToString().ToLower());

            return (await Rest.Put($"channels/{channelId}/permissions/{overwriteId}", data, 
                "channels/channel/permissions/permission").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Deletes a guild channel permission overwrite for a user or role.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> DeletePermission(Snowflake channelId, Snowflake overwriteId)
        {
            return (await Rest.Delete($"channels/{channelId}/permissions/{overwriteId}", 
                "channels/channel/permissions/permission").ConfigureAwait(false)).IsNull;
        }
        #endregion

        #region Message Management
        /// <summary>
        /// Gets messages from a text channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordMessage>> GetMessages(Snowflake channelId,
            Snowflake? baseMessageId = null, int? limit = null,
            DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before)
        {
            string strat = getStrategy.ToString().ToLower();
            string limitStr = limit.HasValue ? $"&limit={limit.Value}" : "";

            DiscordApiData data = await Rest.Get($"channels/{channelId}/messages?{strat}={baseMessageId}{limitStr}", 
                "channels/channel/messages").ConfigureAwait(false);
            DiscordMessage[] messages = new DiscordMessage[data.Values.Count];

            for (int i = 0; i < messages.Length; i++)
                messages[i] = new DiscordMessage(App, data.Values[i]);

            return messages;
        }

        /// <summary>
        /// Gets a single message by ID from a channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordMessage> GetMessage(Snowflake channelId, Snowflake messageId)
        {
            DiscordApiData data = await Rest.Get($"channels/{channelId}/messages/{messageId}", 
                "channels/channel/messages/message").ConfigureAwait(false);
            return new DiscordMessage(App, data);
        }

        #region Deprecated CreateMessage
        /// <summary>
        /// Posts a message to a text channel.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        [Obsolete("Please use overloads using DiscordMessageDetails when creating messages.")]
        public async Task<DiscordMessage> CreateMessage(Snowflake channelId, string content, bool tts = false, Snowflake? nonce = null)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("content", content);
            requestData.Set("tts", tts);
            requestData.Set("nonce", nonce);

            DiscordApiData returnData = await Rest.Post($"channels/{channelId}/messages", requestData,
                "channels/channel/messages").ConfigureAwait(false);
            return new DiscordMessage(App, returnData);
        }
        #endregion

        /// <summary>
        /// Posts a message to a text channel.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> CreateMessage(Snowflake channelId, string content)
        {
            return CreateMessage(channelId, new DiscordMessageDetails(content));
        }

        /// <summary>
        /// Posts a message to a text channel.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="details"/> is null.</exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordMessage> CreateMessage(Snowflake channelId, DiscordMessageDetails details)
        {
            if (details == null)
                throw new ArgumentNullException(nameof(details));

            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("content", details.Content);
            requestData.Set("tts", details.TextToSpeech);
            requestData.Set("nonce", details.Nonce);

            if (details.Embed != null)
                requestData.Set("embed", details.Embed.Build());

            DiscordApiData returnData = await Rest.Post($"channels/{channelId}/messages", requestData,
                "channels/channel/messages").ConfigureAwait(false);
            return new DiscordMessage(App, returnData);
        }

        #region Deprecated UploadFile
        /// <summary>
        /// Uploads a file to a text channel with an optional message.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        [Obsolete("Please use overloads using DiscordMessageDetails when uploading files.")]
        public Task<DiscordMessage> UploadFile(Snowflake channelId, FileInfo fileInfo,
            string message = null, bool tts = false, Snowflake? nonce = null)
        {
            DiscordMessageDetails details = new DiscordMessageDetails();
            details.Content = message;
            details.TextToSpeech = tts;
            details.Nonce = nonce;

            return UploadFile(channelId, new StreamContent(fileInfo.OpenRead()), fileInfo.Name, details);
        }

        /// <summary>
        /// Uploads a file to a text channel with an optional message.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        [Obsolete("Please use overloads using DiscordMessageDetails when uploading files.")]
        public Task<DiscordMessage> UploadFile(Snowflake channelId, byte[] file, string filename = "unknown.jpg",
            string message = null, bool? tts = null, Snowflake? nonce = null)
        {
            DiscordMessageDetails details = new DiscordMessageDetails();
            details.Content = message;
            details.TextToSpeech = tts.GetValueOrDefault();
            details.Nonce = nonce;

            return UploadFile(channelId, new ByteArrayContent(file), filename, details);
        }
        #endregion

        /// <summary>
        /// Uploads a file to a text channel with an optional message.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="fileData"/> is null, 
        /// or if <paramref name="fileName"/> is null or only contains whitespace characters.
        /// </exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> UploadFile(Snowflake channelId, Stream fileData, string fileName, 
            DiscordMessageDetails details = null)
        {
            if (fileData == null)
                throw new ArgumentNullException(nameof(fileData));

            return UploadFile(channelId, new StreamContent(fileData), fileName, details);
        }

        /// <summary>
        /// Uploads a file to a text channel with an optional message.
        /// <para>Note: Bot user accounts must connect to the Gateway at least once before being able to send messages.</para>
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="fileName"/> is null or only contains whitespace characters.
        /// </exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordMessage> UploadFile(Snowflake channelId, ArraySegment<byte> fileData, string fileName,
            DiscordMessageDetails details = null)
        {
            return UploadFile(channelId, new ByteArrayContent(fileData.Array, fileData.Offset, fileData.Count), fileName, details);
        }

        /// <exception cref="ArgumentNullException"></exception>
        async Task<DiscordMessage> UploadFile(Snowflake channelId, HttpContent fileContent, string fileName, DiscordMessageDetails details)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                // Technically this is also handled when setting the field on the multipart form data
                throw new ArgumentNullException(nameof(fileName));

            DiscordApiData returnData = await Rest.Send(() =>
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post,
                    $"{RestClient.BASE_URL}/channels/{channelId}/messages");

                MultipartFormDataContent data = new MultipartFormDataContent();
                data.Add(fileContent, "file", fileName);

                if (details != null)
                {
                    DiscordApiData payloadJson = new DiscordApiData();
                    payloadJson.Set("content", details.Content);
                    payloadJson.Set("tts", details.TextToSpeech);
                    payloadJson.Set("nonce", details.Nonce);

                    if (details.Embed != null)
                        payloadJson.Set("embed", details.Embed.Build());

                    data.Add(new StringContent(payloadJson.SerializeToJson()), "payload_json");
                }

                request.Content = data;

                return request;
            }, "channels/channel/messages").ConfigureAwait(false);
            return new DiscordMessage(App, returnData);
        }

        /// <summary>
        /// Edits an existing message in a text channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordMessage> EditMessage(Snowflake channelId, Snowflake messageId, string content)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("content", content);

            DiscordApiData returnData = await Rest.Patch($"channels/{channelId}/messages/{messageId}", requestData,
                "channels/channel/messages/message").ConfigureAwait(false);
            return new DiscordMessage(App, returnData);
        }

        /// <summary>
        /// Edits an existing message in a text channel.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordMessage> EditMessage(Snowflake channelId, Snowflake messageId, DiscordMessageEdit editDetails)
        {
            if (editDetails == null)
                throw new ArgumentNullException(nameof(editDetails));

            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            requestData.Set("content", editDetails.Content);

            if (editDetails.Embed != null)
                requestData.Set("embed", editDetails.Embed.Build());

            DiscordApiData returnData = await Rest.Patch($"channels/{channelId}/messages/{messageId}", requestData,
                "channels/channel/messages/message").ConfigureAwait(false);
            return new DiscordMessage(App, returnData);
        }

        /// <summary>
        /// Deletes a message from a text channel.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> DeleteMessage(Snowflake channelId, Snowflake messageId)
        {
            DiscordApiData data = await Rest.Delete($"channels/{channelId}/messages/{messageId}", 
                "channels/channel/messages/message/delete").ConfigureAwait(false);
            return data.IsNull;
        }

        /// <summary>
        /// Deletes a group of messages all at once from a text channel.
        /// This is much faster than calling DeleteMessage for each message.
        /// </summary>
        /// <param name="filterTooOldMessages">Whether to ignore deleting messages that are older than 2 weeks (this causes an API error).</param>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<bool> BulkDeleteMessages(Snowflake channelId, IEnumerable<DiscordMessage> messages,
            bool filterTooOldMessages = true)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            List<Snowflake> msgIds = new List<Snowflake>();
            foreach (DiscordMessage msg in messages)
                msgIds.Add(msg.Id);

            return BulkDeleteMessages(channelId, msgIds, filterTooOldMessages);
        }

        /// <summary>
        /// Deletes a group of messages all at once from a text channel.
        /// This is much faster than calling DeleteMessage for each message.
        /// </summary>
        /// <param name="filterTooOldMessages">Whether to ignore deleting messages that are older than 2 weeks (this causes an API error).</param>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> BulkDeleteMessages(Snowflake channelId, IEnumerable<Snowflake> messageIds, 
            bool filterTooOldMessages = true)
        {
            if (messageIds == null)
                throw new ArgumentNullException(nameof(messageIds));

            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            DiscordApiData messages = requestData.Set("messages", new DiscordApiData(DiscordApiDataType.Array));

            ulong minimumAllowedSnowflake = 0;
            if (filterTooOldMessages)
            {
                // See https://github.com/hammerandchisel/discord-api-docs/issues/208

                ulong secondsSinceUnixEpoch = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds();
                minimumAllowedSnowflake = (secondsSinceUnixEpoch - 14 * 24 * 60 * 60) * 1000 - 1420070400000L << 22;
            }

            foreach (Snowflake messageId in messageIds)
            {
                if (!filterTooOldMessages && messageId.Id < minimumAllowedSnowflake)
                    continue;

                messages.Values.Add(new DiscordApiData(messageId));
            }

            DiscordApiData returnData = await Rest.Post($"channels/{channelId}/messages/bulk-delete", requestData, 
                "channels/channel/messages/message/delete/bulk").ConfigureAwait(false);
            return returnData.IsNull;
        }

        /// <summary>
        /// Gets a list of all pinned messages in a text channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordMessage>> GetPinnedMessages(Snowflake channelId)
        {
            DiscordApiData data = await Rest.Get($"channels/{channelId}/pins", 
                "channels/channel/pins").ConfigureAwait(false);
            DiscordMessage[] messages = new DiscordMessage[data.Values.Count];

            for (int i = 0; i < messages.Length; i++)
                messages[i] = new DiscordMessage(App, data.Values[i]);

            return messages;
        }

        /// <summary>
        /// Pins a message in a text channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> AddPinnedMessage(Snowflake channelId, Snowflake messageId)
        {
            return (await Rest.Put($"channels/{channelId}/pins/{messageId}", 
                "channels/channel/pins/message").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Unpins a message from a text channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> DeletePinnedMessage(Snowflake channelId, Snowflake messageId)
        {
            return (await Rest.Delete($"channels/{channelId}/pins/{messageId}", 
                "channels/channel/pins/message").ConfigureAwait(false)).IsNull;
        }
        #endregion

        #region Reaction Management
        /// <summary>
        /// Adds a reaction to a message.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> CreateReaction(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            return (await Rest.Put($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me", 
                "channels/channel/messages/message/reactions/emoji/@me").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Deletes a reaction the currently authenticated user has made for a message.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> DeleteOwnReaction(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            return (await Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/@me",
                "channels/channel/messages/message/reactions/emoji/@me").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Deletes a reaction posted by any user.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> DeleteUserReaction(Snowflake channelId, Snowflake messageId, Snowflake userId, DiscordReactionEmoji emoji)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            return (await Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions/{emoji}/{userId}",
                "channels/channel/messages/message/reactions/emoji/user").ConfigureAwait(false)).IsNull;
        }

        /// <summary>
        /// Gets a list of all users who reacted to the specified message with the specified emoji.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordUser>> GetReactions(Snowflake channelId, Snowflake messageId, DiscordReactionEmoji emoji)
        {
            if (emoji == null)
                throw new ArgumentNullException(nameof(emoji));

            DiscordApiData data = await Rest.Get($"channels/{channelId}/messages/{messageId}/reactions/{emoji}",
                "channels/channel/messages/message/reactions/emoji").ConfigureAwait(false);

            DiscordUser[] users = new DiscordUser[data.Values.Count];
            for (int i = 0; i < users.Length; i++)
                users[i] = new DiscordUser(data.Values[i]);

            return users;
        }

        /// <summary>
        /// Deletes all reactions on a message.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task DeleteAllReactions(Snowflake channelId, Snowflake messageId)
        {
            await Rest.Delete($"channels/{channelId}/messages/{messageId}/reactions",
                "channels/channel/messages/message/reactions").ConfigureAwait(false);
        }
        #endregion

        #region Invite Management
        /// <summary>
        /// Gets a list of all invites for the specified guild channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<IReadOnlyList<DiscordInviteMetadata>> GetInvites(Snowflake channelId)
        {
            DiscordApiData data = await Rest.Get($"channels/{channelId}/invites", 
                "channels/channel/invites").ConfigureAwait(false);

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
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<DiscordInvite> CreateInvite(Snowflake channelId,
            TimeSpan? maxAge = null, int? maxUses = null, bool? temporary = null, bool? unique = null)
        {
            DiscordApiData requestData = new DiscordApiData(DiscordApiDataType.Container);
            if (maxAge.HasValue) requestData.Set("max_age", maxAge.Value.Seconds);
            if (maxUses.HasValue) requestData.Set("max_uses", maxUses.Value);
            if (temporary.HasValue) requestData.Set("temporary", temporary.Value);
            if (unique.HasValue) requestData.Set("unique", unique.Value);

            DiscordApiData returnData = await Rest.Post($"channels/{channelId}/invites", requestData,
                "channels/channel/invites").ConfigureAwait(false);
            return new DiscordInvite(App, returnData);
        }
        #endregion

        /// <summary>
        /// Causes the current authenticated user to appear as typing in this channel.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public async Task<bool> TriggerTypingIndicator(Snowflake channelId)
        {
            return (await Rest.Post($"channels/{channelId}/typing", 
                "channels/channel/typing").ConfigureAwait(false)).IsNull;
        }
    }
}
