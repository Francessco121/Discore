using Discore.Http.Net;

namespace Discore.WebSocket
{
    public sealed class DiscordGuildTextChannel : DiscordGuildChannel
    {
        /// <summary>
        /// Gets the topic of this channel.
        /// </summary>
        public string Topic { get; private set; }

        /// <summary>
        /// Gets the id of the last message sent in this channel.
        /// </summary>
        public string LastMessageId { get; private set; }

        HttpChannelsEndpoint channelsHttp;

        internal DiscordGuildTextChannel(Shard shard, DiscordGuild guild)
            : base(shard, guild, DiscordGuildChannelType.Text)
        {
            channelsHttp = shard.Application.InternalHttpApi.Channels;
        }

        /// <summary>
        /// Sends a message to this guild message channel.
        /// Returns the message sent, or null if the send failed.
        /// </summary>
        public DiscordMessage SendMessage(string content)
        {
            DiscordApiData data = channelsHttp.Create(Id, content);

            if (data != null && !data.IsNull)
            {
                DiscordMessage msg = new DiscordMessage(Shard);
                msg.Update(data);

                return msg;
            }
            else
                return null;
        }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            Topic = data.GetString("topic") ?? Topic;
            LastMessageId = data.GetString("last_message_id") ?? LastMessageId;
        }
    }
}
