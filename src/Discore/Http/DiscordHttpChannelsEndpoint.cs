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

        public async Task<T> GetChannel<T>(Snowflake Id) where T : DiscordChannel
         => Activator.CreateInstance(typeof(T), await endpoint.Get(Id)) as T;

        /// <summary>
        /// Modify a Discord Channel
        /// </summary>
        /// <param name="channel">The <see cref="DiscordChannel"/> to modify</param>
        /// <param name="name">New name</param>
        /// <param name="position">Position as it appears in the client</param>
        /// <param name="topic"/>
        /// <param name="bitrate">Voice bitrate limit; if its a <see cref="DiscordGuildChannelType.Voice"/></param>
        /// <param name="userLimit"/>
        public async Task<T> ModifyChannel<T>(T channel, string name = null, int? position = null,
            string topic = null, int? bitrate = null, int? userLimit = null) where T : DiscordChannel
             => Activator.CreateInstance(typeof(T), await endpoint.Modify(channel.Id, name, position, topic, bitrate, userLimit)) as T;
            //=> new T(await endpoint.Modify(channel.Id, name, position, topic, bitrate, userLimit));

        public async Task DeleteChannel(DiscordChannel channel)
            => await endpoint.Delete(channel.Id);
        #endregion

        #region Message Management

        public async Task<DiscordMessage> GetMessage(DiscordChannel channel, DiscordMessage message)
            => await GetMessage(channel, message.Id);

        public async Task<DiscordMessage> GetMessage(DiscordChannel channel, Snowflake messageId)
            => new DiscordMessage(await endpoint.GetMessage(channel.Id, messageId));

        public async Task<IEnumerable<DiscordMessage>> GetMessages(DiscordChannel channel, DiscordMessage baseMessage = null, int? limit = null,
            DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before)
        {
            List<DiscordMessage> toReturn = new List<DiscordMessage>();
            DiscordApiData data = await endpoint.GetMessages(channel.Id, baseMessage.Id, limit, getStrategy);
            foreach (DiscordApiData item in data.Values)
                toReturn.Add(new DiscordMessage(item));

            return toReturn;
        }

        public async Task<DiscordMessage> CreateMessage(DiscordChannel channel, string content, bool tts = false, DiscordIdObject nonce = null)
            => new DiscordMessage(await endpoint.CreateMessage(channel.Id, content, tts, nonce?.Id));

        public async Task<DiscordMessage> CreateMessage(DiscordChannel channel, string content, byte[] file, string filename = null, bool tts = false,
            DiscordIdObject nonce = null)
            => new DiscordMessage(await endpoint.CreateMessage(channel.Id, content, file, filename, tts, nonce?.Id));

        public async Task<DiscordMessage> CreateMessage(DiscordChannel channel, string content, FileInfo file, bool tts = false, DiscordIdObject nonce = null)
        {
            using (FileStream fs = file.OpenRead())
            using (MemoryStream ms = new MemoryStream())
            {
                await ms.CopyToAsync(ms);
                return new DiscordMessage(await endpoint.CreateMessage(channel.Id, content, ms.ToArray(), file.Name, tts, nonce?.Id));
            }
        }

        public async Task<DiscordMessage> EditMessage(DiscordMessage message, string content)
            => new DiscordMessage(await endpoint.EditMessage(message.ChannelId, message.Id, content));

        public async Task DeleteMessage(DiscordMessage message)
            => await endpoint.DeleteMessage(message.ChannelId, message.Id);

        public async Task DeleteMessages(DiscordChannel channel, IEnumerable<DiscordMessage> messages)
        {
            List<Snowflake> msgIds = new List<Snowflake>(messages.Count());
            foreach (DiscordMessage msg in messages)
                msgIds.Add(msg.Id);

            await endpoint.BulkDeleteMessages(channel.Id, msgIds);
        }

        #endregion

        #region Reaction Management

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
        {
            List<DiscordInvite> toReturn = new List<DiscordInvite>();
            DiscordApiData data = await endpoint.GetInvites(channel.Id);
            foreach (DiscordApiData item in data.Values)
                toReturn.Add(new DiscordInvite(item));

            return toReturn;
        }

        public async Task<DiscordInvite> CreateInvite(DiscordChannel channel,
            TimeSpan? maxAge = null, int maxUses = 0, bool temporary = false, bool unique = false)
        {
            if (!maxAge.HasValue) maxAge = TimeSpan.FromHours(24); //we default to 24 hours, just like Discord's api
            return new DiscordInvite(await endpoint.CreateInvite(channel.Id, maxAge.Value.Seconds, maxUses, temporary, unique));
        }

        #endregion

        #region Pinned Messages Management

        public async Task<IEnumerable<DiscordMessage>> GetPinnedMessages(DiscordChannel channel)
        {
            List<DiscordMessage> toReturn = new List<DiscordMessage>();
            DiscordApiData data = await endpoint.GetPinnedMessages(channel.Id);
            foreach (DiscordApiData item in data.Values)
                toReturn.Add(new DiscordMessage(item));

            return toReturn;
        }

        public async Task PinMessage(DiscordMessage message)
            => await endpoint.AddPinnedChannelMessage(message.ChannelId, message.Id);

        public async Task UnpinMessage(DiscordMessage message)
            => await endpoint.DeletePinnedChannelMessage(message.ChannelId, message.Id);

        #endregion

        #region Extra

        public async Task<DiscordApiData> StartTyping(DiscordChannel channel)
            => new DiscordApiData(await endpoint.TriggerTypingIndicator(channel.Id));

        #endregion
    }
}
