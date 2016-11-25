using Discore.Http.Net;
using System;
using System.Threading.Tasks;

namespace Discore.WebSocket
{
    /// <summary>
    /// A <see cref="DiscordDMChannel"/> or a <see cref="DiscordGuildChannel"/>.
    /// </summary>
    public abstract class DiscordChannel : DiscordIdObject
    {
        /// <summary>
        /// Gets the type of this channel.
        /// </summary>
        public DiscordChannelType ChannelType { get; }

        protected Shard Shard { get; }

        HttpChannelsEndpoint channelsHttp;

        internal DiscordChannel(Shard shard, DiscordChannelType type)
        {
            Shard = shard;
            ChannelType = type;

            channelsHttp = shard.Application.InternalHttpApi.Channels;
        }

        /// <summary>
        /// Deletes/closes this channel.
        /// </summary>
        public void Delete()
        {
            try { DeleteAsync().Wait(); }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Deletes/closes this channel.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> DeleteAsync()
        {
            DiscordApiData data = await channelsHttp.Delete(Id);
            return data.IsNull;
        }
    }
}
