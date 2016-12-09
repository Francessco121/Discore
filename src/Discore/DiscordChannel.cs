using Discore.Http.Net;
using System;
using System.Threading.Tasks;

namespace Discore
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

        HttpChannelsEndpoint channelsHttp;

        internal DiscordChannel(IDiscordApplication app, DiscordApiData data, DiscordChannelType type)
            : base(data)
        {
            ChannelType = type;
            channelsHttp = app.HttpApi.InternalApi.Channels;
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
