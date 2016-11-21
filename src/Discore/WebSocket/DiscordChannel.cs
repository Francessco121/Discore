using Discore.Http.Net;
using System;

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
        /// <returns>Returns whether the operation was successful.</returns>
        public bool Delete()
        {
            DiscordApiData data = channelsHttp.Delete(Id);
            return data.IsNull;
        }
    }
}
