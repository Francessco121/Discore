using System;

namespace Discore.WebSocket
{
    /// <summary>
    /// A set of options to use when starting a shard.
    /// </summary>
    public class ShardStartConfig
    {
        /// <summary>
        /// Gets or sets a value between 50 and 250, which represents the number of members
        /// where the Gateway will consider a guild to be "large". Default: 250.
        /// <para>
        /// "Large" guilds will not have their offline members immediately available, 
        /// and instead must be requested.
        /// </para>
        /// </summary>
        public int GatewayLargeThreshold
        {
            get => largeThreshold;
            set
            {
                if (value < 50 || value > 250)
                    throw new ArgumentOutOfRangeException(nameof(value), "Large threshold must be between 50 and 250.");

                largeThreshold = value;
            }
        }

        int largeThreshold = 250;
    }
}
