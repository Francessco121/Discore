using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discore.Http.Net;

namespace Discore.Http
{
    public sealed class DiscordHttpChannelsEndpoint
    {
        HttpChannelsEndpoint endpoint;

        internal DiscordHttpChannelsEndpoint(HttpChannelsEndpoint endpoint)
        {
            this.endpoint = endpoint;
        }

        #region Channel Management

        DiscordChannel GetChannelAsProperChannel(DiscordApiData data)
        {
            DiscordChannel toReturn = null;

            bool? isPrivate = data.GetBoolean("is_private");
            string channelType = data.GetString("type");

            if (isPrivate.HasValue)
            {
                if (!isPrivate.Value && !string.IsNullOrWhiteSpace(channelType))
                {
                    if (channelType == "voice") // if voice channel
                        toReturn = new DiscordGuildVoiceChannel(data);

                    // else we assume text channel
                    toReturn = new DiscordGuildTextChannel(data);
                }
                else if (isPrivate.Value)
                    toReturn = new DiscordDMChannel(data);
            }

            throw new NotSupportedException($"{nameof(Snowflake)} isn't a known type of {nameof(DiscordChannel)} or something messed up bigtime.");
        }

        /// <summary>
        /// Get a Discord Channel
        /// </summary>
        /// <typeparam name="T">Type to cast to</typeparam>
        /// <param name="Id">Id of channel</param>
        /// <returns></returns>
        public async Task<T> GetChannel<T>(Snowflake Id) 
            where T : DiscordChannel
        {
            DiscordApiData data = await endpoint.Get(Id);
            return (T)GetChannelAsProperChannel(data);
        }

        /// <summary>
        /// Modify a Discord Channel
        /// </summary>
        /// <param name="channel">The <see cref="DiscordChannel"/> to modify</param>
        /// <param name="name">New name</param>
        /// <param name="position">Position as it appears in the client</param>
        /// <param name="topic"/>
        /// <param name="bitrate">Voice bitrate limit; if its a <see cref="DiscordGuildChannelType.Voice"/></param>
        /// <param name="userLimit"/>
        public async Task<T> ModifyChannel<T>(
            T channel,
            string name = null,
            int? position = null,
            string topic = null,
            int? bitrate = null,
            int? userLimit = null)
            where T : DiscordChannel
        {
            DiscordApiData data = await endpoint.Modify(channel.Id, name, position, topic, bitrate, userLimit);
            return (T)GetChannelAsProperChannel(data);            
        }

        public async Task DeleteChannel(DiscordChannel channel)
            => await endpoint.Delete(channel.Id);
        #endregion

        #region Message Management

        public async Task<DiscordMessage> GetMessage(DiscordChannel channel, DiscordMessage message)
            => await GetMessage(channel.Id, message.Id);

        public async Task<DiscordMessage> GetMessage(Snowflake channelId, Snowflake messageId)
            => new DiscordMessage(await endpoint.GetMessage(channelId, messageId));

        public async Task<IEnumerable<DiscordMessage>> GetMessages(DiscordChannel channel,
            DiscordMessage baseMessage = null,
            int? limit = null,
           DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before)
            => await GetMessages(channel.Id, baseMessage?.Id, limit, getStrategy);

        public async Task<IEnumerable<DiscordMessage>> GetMessages(Snowflake channelId, 
            Snowflake? baseMessageId = null, 
            int? limit = null,
            DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before)
        {
            List<DiscordMessage> toReturn = new List<DiscordMessage>();
            DiscordApiData data = await endpoint.GetMessages(channelId, baseMessageId, limit, getStrategy);
            foreach (DiscordApiData item in data.Values)
                toReturn.Add(new DiscordMessage(item));

            return toReturn;
        }

        public async Task<DiscordMessage> CreateMessage(DiscordChannel channel, string content, bool tts = false, DiscordIdObject nonce = null)
            => await CreateMessage(channel.Id, content, tts, nonce);

        public async Task<DiscordMessage> CreateMessage(Snowflake channelId, string content, bool tts = false, DiscordIdObject nonce = null)
            => new DiscordMessage(await endpoint.CreateMessage(channelId, content, tts, nonce?.Id));

        public async Task<DiscordMessage> UploadFile(DiscordChannel channel, string content, byte[] file, string filename = "unknown.jpg", bool tts = false,
           DiscordIdObject nonce = null)
            => await UploadFile(channel.Id, content, file, filename, tts, nonce);

        public async Task<DiscordMessage> UploadFile(Snowflake channelId, string content, byte[] file, string filename = "unknown.jpg", bool tts = false,
            DiscordIdObject nonce = null)
            => new DiscordMessage(await endpoint.UploadFile(channelId, file, filename, content, tts, nonce?.Id));

        public async Task<DiscordMessage> UploadFile(DiscordChannel channel, string content, FileInfo file, bool tts = false, DiscordIdObject nonce = null)
            => await UploadFile(channel.Id, content, file, tts, nonce);

        public async Task<DiscordMessage> UploadFile(Snowflake channelId, string content, FileInfo file, bool tts = false, DiscordIdObject nonce = null)
        {
            using (FileStream fs = file.OpenRead())
            using (MemoryStream ms = new MemoryStream())
            {
                await ms.CopyToAsync(ms);
                return new DiscordMessage(await endpoint.UploadFile(channelId, ms.ToArray(), file.Name, content, tts, nonce?.Id));
            }
        }

        public async Task<DiscordMessage> EditMessage(DiscordMessage message, string content)
            => await EditMessage(message.ChannelId, message.Id, content);

        public async Task<DiscordMessage> EditMessage(Snowflake channelId, Snowflake messageId, string content)
            => new DiscordMessage(await endpoint.EditMessage(channelId, messageId, content));

        public async Task DeleteMessage(DiscordMessage message)
            => await DeleteMessage(message.ChannelId, message.Id);

        public async Task DeleteMessage(Snowflake channelId, Snowflake messageId)
            => await endpoint.DeleteMessage(channelId, messageId);

        public async Task DeleteMessages(DiscordChannel channel, IEnumerable<DiscordMessage> messages)
        {
            List<Snowflake> msgIds = new List<Snowflake>(messages.Count());
            foreach (DiscordMessage msg in messages)
                msgIds.Add(msg.Id);

            await DeleteMessages(channel.Id, msgIds);
        }

        public async Task DeleteMessages(Snowflake channelId, IEnumerable<Snowflake> messages)
            => await endpoint.BulkDeleteMessages(channelId, messages);

        #endregion

        #region Reaction Management

        /// <summary>
        /// Gets a enumerable list of <see cref="DiscordUser"/> that reacted to a <see cref="DiscordMessage"/> using a particular <see cref="DiscordEmoji"/>
        /// </summary>
        /// <param name="message"><see cref="DiscordMessage"/> to check</param>
        /// <param name="emoji"><see cref="DiscordEmoji"/> to search for</param>
        /// <returns></returns>
        public async Task<IEnumerable<DiscordUser>> GetUsersThatReacted(DiscordMessage message, DiscordReactionEmoji emoji)
        {
            List<DiscordUser> toReturn = new List<DiscordUser>();
            DiscordApiData data = await endpoint.GetReactions(message.ChannelId, message.Id, emoji);
            foreach (DiscordApiData item in data.Values)
                toReturn.Add(new DiscordUser(item));

            return toReturn;
        }

        public async Task AddReaction(DiscordMessage message, DiscordReactionEmoji emoji)
            => await endpoint.CreateReaction(message.ChannelId, message.Id, emoji);

        public async Task RemoveReaction(DiscordMessage message, DiscordReactionEmoji emoji, DiscordUser owner = null)
        {
            if (owner != null) //we are deleting someone else's (or our own) reaction
                await endpoint.DeleteUserReaction(message.ChannelId, message.Id, owner.Id, emoji);
            else //we are deff deleting our own
                await endpoint.DeleteOwnReaction(message.ChannelId, message.Id, emoji);
        }

        public async Task ClearAllReactions(DiscordMessage message)
            => await endpoint.DeleteAllReactions(message.ChannelId, message.Id);

        #endregion

        #region Invite Management

        public async Task<IEnumerable<DiscordInvite>> GetInvites(DiscordChannel channel)
            => await GetInvites(channel.Id);

        public async Task<IEnumerable<DiscordInvite>> GetInvites(Snowflake Id)
        {
            List<DiscordInvite> toReturn = new List<DiscordInvite>();
            DiscordApiData data = await endpoint.GetInvites(Id);
            foreach (DiscordApiData item in data.Values)
                toReturn.Add(new DiscordInvite(item));

            return toReturn;
        }

        public async Task<DiscordInvite> CreateInvite(DiscordChannel channel,
            TimeSpan? maxAge = null, int maxUses = 0, bool temporary = false, bool unique = false)
            => await CreateInvite(channel.Id, maxAge, maxUses, temporary, unique);

        public async Task<DiscordInvite> CreateInvite(Snowflake Id,
            TimeSpan? maxAge = null, int maxUses = 0, bool temporary = false, bool unique = false)
        {
            if (!maxAge.HasValue) maxAge = TimeSpan.FromHours(24); //we default to 24 hours, just like Discord's api
            return new DiscordInvite(await endpoint.CreateInvite(Id, maxAge.Value.Seconds, maxUses, temporary, unique));
        }

        #endregion

        #region Pinned Messages Management

        public async Task<IEnumerable<DiscordMessage>> GetPinnedMessages(DiscordChannel channel)
            => await GetPinnedMessages(channel.Id);

        public async Task<IEnumerable<DiscordMessage>> GetPinnedMessages(Snowflake Id)
        {
            List<DiscordMessage> toReturn = new List<DiscordMessage>();
            DiscordApiData data = await endpoint.GetPinnedMessages(Id);
            foreach (DiscordApiData item in data.Values)
                toReturn.Add(new DiscordMessage(item));

            return toReturn;
        }

        public async Task PinMessage(DiscordMessage message)
            => await PinMessage(message.ChannelId, message.Id);

        public async Task PinMessage(Snowflake channelId, Snowflake messageId)
            => await endpoint.AddPinnedChannelMessage(channelId, messageId);

        public async Task UnpinMessage(DiscordMessage message)
            => await UnpinMessage(message.ChannelId, message.Id);

        public async Task UnpinMessage(Snowflake channelId, Snowflake messageId)
            => await endpoint.DeletePinnedChannelMessage(channelId, messageId);

        #endregion

        #region Extra
        public async Task<bool> StartTyping(DiscordChannel channel)
            => await StartTyping(channel.Id);

        public async Task<bool> StartTyping(Snowflake Id)
            => (await endpoint.TriggerTypingIndicator(Id)).IsNull;

        #endregion
    }
}
