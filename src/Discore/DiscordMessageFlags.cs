using System;

namespace Discore
{
    [Flags]
    public enum DiscordMessageFlags
    {
        None = 0,
        /// <summary>
        /// This message has been published to other channels.
        /// </summary>
        Crossposted = 1 << 0,
        /// <summary>
        /// This message originated from a message in another channel.
        /// </summary>
        IsCrosspost = 1 << 1,
        /// <summary>
        /// Do not include any embeds when serializing this message.
        /// </summary>
        SuppressEmbeds = 1 << 2
    }
}
