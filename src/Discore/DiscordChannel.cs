using Discore.Http;
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

        DiscordHttpApi http;

        internal DiscordChannel(DiscordHttpApi http, DiscordChannelType type)
        {
            this.http = http;
            ChannelType = type;
        }

        internal DiscordChannel(DiscordHttpApi http, DiscordApiData data, DiscordChannelType type)
            : base(data)
        {
            this.http = http;
            ChannelType = type;
        }

        /// <summary>
        /// Deletes/closes this channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordChannel> Delete()
        {
            return http.DeleteChannel(Id);
        }
    }
}
