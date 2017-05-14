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

        internal DiscordChannel(IDiscordApplication app, DiscordApiData data, DiscordChannelType type)
            : base(data)
        {
            ChannelType = type;
            http = app.HttpApi;
        }

        /// <summary>
        /// Deletes/closes this channel.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordChannel> Delete()
        {
            return http.DeleteChannel(Id);
        }
    }
}
