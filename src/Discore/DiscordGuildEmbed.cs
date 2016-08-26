namespace Discore
{
    public class DiscordGuildEmbed : IDiscordObject
    {
        public bool Enabled { get; private set; }
        public DiscordChannel Channel { get; private set; }

        DiscordApiCache cache;

        public DiscordGuildEmbed(IDiscordClient client)
        {
            cache = client.Cache;
        }

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
