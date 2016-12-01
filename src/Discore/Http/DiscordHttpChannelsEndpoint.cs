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

        public async Task<DiscordApiData> GetChannel(DiscordChannel channel)
            => new DiscordApiData(await endpoint.Get(channel.Id));

        /// <summary>
        /// Modify a Discord Channel
        /// </summary>
        /// <param name="channel">The <see cref="DiscordChannel"/> to modify</param>
        /// <param name="name">New name</param>
        /// <param name="position">Position as it appears in the client</param>
        /// <param name="topic"/>
        /// <param name="bitrate">Voice bitrate limit; if its a <see cref="DiscordGuildChannelType.Voice"/></param>
        /// <param name="userLimit"/>
        public async Task<DiscordApiData> ModifyChannel(DiscordChannel channel, string name = null, int? position = null,
            string topic = null, int? bitrate = null, int? userLimit = null)
            => new DiscordApiData(await endpoint.Modify(channel.Id, name, position, topic, bitrate, userLimit));

        public async Task<DiscordApiData> DeleteChannel(DiscordChannel channel)
            => new DiscordApiData(await endpoint.Delete(channel.Id));
        #endregion

        #region Message Management

        public async Task<DiscordApiData> GetMessage(DiscordChannel channel, DiscordMessage message)
            => new DiscordApiData(await endpoint.GetMessage(channel.Id, message.Id));

        public async Task<DiscordApiData> GetMessages(DiscordChannel channel, DiscordMessage baseMessage = null, int? limit = null,
            DiscordMessageGetStrategy getStrategy = DiscordMessageGetStrategy.Before)
            => new DiscordApiData(await endpoint.GetMessages(channel.Id, baseMessage.Id, limit, getStrategy));

        public async Task<DiscordApiData> CreateMessage(DiscordChannel channel, string content, bool tts = false, DiscordIdObject nonce = null)
            => new DiscordApiData(await endpoint.CreateMessage(channel.Id, content, tts, nonce?.Id));

        public async Task<DiscordApiData> CreateMessage(DiscordChannel channel, string content, byte[] file, string filename = null, bool tts = false,
            DiscordIdObject nonce = null)
            => new DiscordApiData(await endpoint.CreateMessage(channel.Id, content, file, filename, tts, nonce?.Id));

        public async Task<DiscordApiData> CreateMessage(DiscordChannel channel, string content, FileInfo file, bool tts = false, DiscordIdObject nonce = null)
        {
            using (FileStream fs = file.OpenRead())
            using (MemoryStream ms = new MemoryStream())
            {
                await ms.CopyToAsync(ms);
                return new DiscordApiData(await endpoint.CreateMessage(channel.Id, content, ms.ToArray(), file.Name, tts, nonce?.Id));
            }
        }

        public async Task<DiscordApiData> EditMessage(DiscordMessage message, string content)
            => new DiscordApiData(await endpoint.EditMessage(message.ChannelId, message.Id, content));

        public async Task<DiscordApiData> DeleteMessage(DiscordMessage message)
            => new DiscordApiData(await endpoint.DeleteMessage(message.ChannelId, message.Id));

        public async Task<DiscordApiData> DeleteMessages(DiscordChannel channel, IEnumerable<DiscordMessage> messages)
        {
            List<Snowflake> msgIds = new List<Snowflake>(messages.Count());
            foreach (DiscordMessage msg in messages)
                msgIds.Add(msg.Id);

            return new DiscordApiData(await endpoint.BulkDeleteMessages(channel.Id, msgIds));
        }

        #endregion

        #region Reaction Management

        public async Task<DiscordApiData> GetUsersThatReacted(DiscordMessage message, DiscordReactionEmoji emoji)
            => new DiscordApiData(await endpoint.GetReactions(message.ChannelId, message.Id, emoji));

        public async Task<DiscordApiData> AddReaction(DiscordMessage message, DiscordReactionEmoji emoji)
            => new DiscordApiData(await endpoint.CreateReaction(message.ChannelId, message.Id, emoji));

        public async Task<DiscordApiData> RemoveReaction(DiscordMessage message, DiscordReactionEmoji emoji, DiscordUser owner = null)
        {
            if (owner != null) //we are deleting someone else's (or our own) reaction
                return new DiscordApiData(await endpoint.DeleteUserReaction(message.ChannelId, message.Id, owner.Id, emoji));
            else //we are deff deleting our own
                return new DiscordApiData(await endpoint.DeleteOwnReaction(message.ChannelId, message.Id, emoji));
        }

        public async Task<DiscordApiData> ClearAllReactions(DiscordMessage message)
            => new DiscordApiData(await endpoint.DeleteAllReactions(message.ChannelId, message.Id));

        #endregion

        #region Invite Management

        public async Task<DiscordApiData> GetInvites(DiscordChannel channel)
            => new DiscordApiData(await endpoint.GetInvites(channel.Id));

        public async Task<DiscordApiData> CreateInvite(DiscordChannel channel,
            TimeSpan? maxAge = null, int maxUses = 0, bool temporary = false, bool unique = false)
        {
            if (!maxAge.HasValue) maxAge = TimeSpan.FromHours(24); //we default to 24 hours, just like Discord's api
            return new DiscordApiData(await endpoint.CreateInvite(channel.Id, maxAge.Value.Seconds, maxUses, temporary, unique));
        }

        #endregion

        #region Pinned Messages Management

        public async Task<DiscordApiData> GetPinnedMessages(DiscordChannel channel)
            => new DiscordApiData(await endpoint.GetPinnedMessages(channel.Id));

        public async Task<DiscordApiData> PinMessage(DiscordMessage message)
            => new DiscordApiData(await endpoint.AddPinnedChannelMessage(message.ChannelId, message.Id));

        public async Task<DiscordApiData> UnpinMessage(DiscordMessage message)
            => new DiscordApiData(await endpoint.DeletePinnedChannelMessage(message.ChannelId, message.Id));

        #endregion

        #region Extra

        public async Task<DiscordApiData> StartTyping(DiscordChannel channel)
            => new DiscordApiData(await endpoint.TriggerTypingIndicator(channel.Id));

        #endregion
    }
}
