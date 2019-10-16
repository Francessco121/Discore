using Discore.Http;
using System.Threading.Tasks;

namespace Discore
{
    /// <summary>
    /// A <see cref="DiscordDMChannel"/> or a <see cref="DiscordGuildChannel"/>.
    /// </summary>
    public abstract class DiscordChannel : DiscordIdEntity
    {
        /// <summary>
        /// Gets the type of this channel.
        /// </summary>
        public DiscordChannelType ChannelType { get; }

        /// <summary>
        /// Gets whether this channel is a guild channel.
        /// </summary>
        public bool IsGuildChannel => 
               ChannelType == DiscordChannelType.GuildText
            || ChannelType == DiscordChannelType.GuildVoice
            || ChannelType == DiscordChannelType.GuildCategory
            || ChannelType == DiscordChannelType.GuildNews
            || ChannelType == DiscordChannelType.GuildStore;

        readonly DiscordHttpClient http;

        internal DiscordChannel(DiscordHttpClient http, DiscordChannelType type)
        {
            this.http = http;
            ChannelType = type;
        }

        internal DiscordChannel(DiscordHttpClient http, DiscordApiData data, DiscordChannelType type)
            : base(data)
        {
            this.http = http;
            ChannelType = type;
        }

        /// <summary>
        /// Deletes/closes this channel.
        /// <para>Requires <see cref="DiscordPermission.ManageChannels"/> if this is a guild channel.</para>
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordChannel> Delete()
        {
            return http.DeleteChannel(Id);
        }

        public override string ToString()
        {
            return $"{ChannelType} Channel: {Id}";
        }
    }
}
