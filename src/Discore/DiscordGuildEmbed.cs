namespace Discore
{
    /// <summary>
    /// An embedded guild widget.
    /// </summary>
    public class DiscordGuildEmbed : IDiscordObject
    {
        /// <summary>
        /// Gets whether or not this guild embed is enabled.
        /// </summary>
        public bool Enabled { get; private set; }
        /// <summary>
        /// Gets the channel this embed is associated with.
        /// </summary>
        public DiscordChannel Channel { get; private set; }

        DiscordApiCache cache;

        /// <summary>
        /// Creates a new <see cref="DiscordGuildEmbed"/> instance.
        /// </summary>
        /// <param name="client">The associated <see cref="IDiscordClient"/>.</param>
        public DiscordGuildEmbed(IDiscordClient client)
        {
            cache = client.Cache;
        }

        /// <summary>
        /// Updates this guild embed with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this guild embed with.</param>
        public void Update(DiscordApiData data)
        {
            Enabled = data.GetBoolean("enabled") ?? Enabled;

            string channelId = data.GetString("channel_id");
            if (channelId != null)
            {
                DiscordChannel channel;
                if (cache.TryGet(channelId, out channel))
                    Channel = channel;
                else
                    DiscordLogger.Default.LogWarning($"[GUILD_EMBED.UPDATE] Failed to locate channel with id {channelId}");
            }
        }
    }
}
