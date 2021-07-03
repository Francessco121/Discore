using System;

namespace Discore.WebSocket
{
    /// <summary>
    /// A set of options to use when starting a shard.
    /// </summary>
    public class ShardStartConfig
    {
        /// <summary>
        /// A bitwise OR of Gateway event groups to subscribe to. Default: none.
        /// <para/>
        /// Gateway events in groups that are not specified here will never be fired.
        /// For example, to receive 'MessageCreate' events for guild messages, specify <see cref="GatewayIntent.GuildMessages"/>.
        /// </summary>
        public GatewayIntent Intents { get; set; }

        /// <summary>
        /// Gets or sets a value between 50 and 250, which represents the number of members
        /// where the Gateway will consider a guild to be "large". Default: 50.
        /// <para>
        /// "Large" guilds will not have their offline members immediately available, 
        /// and instead must be requested.
        /// </para>
        /// </summary>
        public int? GatewayLargeThreshold
        {
            get => largeThreshold;
            set
            {
                if (value != null && (value.Value < 50 || value.Value > 250))
                    throw new ArgumentOutOfRangeException(nameof(value), "Large threshold must be between 50 and 250.");

                largeThreshold = value;
            }
        }

        int? largeThreshold;
    }
}
