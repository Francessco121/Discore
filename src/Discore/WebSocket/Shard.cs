using Discore.WebSocket.Net;
using System;

namespace Discore.WebSocket
{
    public class Shard : IDisposable
    {
        public int ShardId { get; }
        public DiscordWebSocketApplication Application { get; }
        public bool IsActive { get { return isRunning; } }

        /// <summary>
        /// Gets a table of all guilds managed by this shard.
        /// </summary>
        public DiscordApiCacheTable<DiscordGuild> Guilds { get; }
        /// <summary>
        /// Gets a table of all channels managed by this shard.
        /// </summary>
        public DiscordApiCacheTable<DiscordChannel> Channels { get; }
        /// <summary>
        /// Gets a table of all roles managed by this shard.
        /// </summary>
        public DiscordApiCacheTable<DiscordRole> Roles { get; }
        /// <summary>
        /// Gets a table of all DM channels managed by this shard.
        /// </summary>
        public DiscordApiCacheTable<DiscordDMChannel> DirectMessageChannels { get; }
        /// <summary>
        /// Gets a table of all users managed by this shard.
        /// </summary>
        public DiscordApiCacheTable<DiscordUser> Users { get; }

        /// <summary>
        /// Gets the user used to authenticate this shard connection.
        /// Or null if the gateway is not currently connected.
        /// </summary>
        public DiscordUser User { get; internal set; }

        /// <summary>
        /// Gets the gateway manager for this shard.
        /// </summary>
        public IDiscordGateway Gateway { get { return gateway; } }

        bool isRunning;
        Gateway gateway;
        DiscoreLogger log;

        internal Shard(DiscordWebSocketApplication app, int shardId)
        {
            Application = app;
            ShardId = shardId;

            log = new DiscoreLogger($"Shard#{shardId}");

            Guilds = new DiscordApiCacheTable<DiscordGuild>();
            Channels = new DiscordApiCacheTable<DiscordChannel>();
            Roles = new DiscordApiCacheTable<DiscordRole>();
            DirectMessageChannels = new DiscordApiCacheTable<DiscordDMChannel>();
            Users = new DiscordApiCacheTable<DiscordUser>();

            gateway = new Gateway(app, this);
        }

        internal bool Start()
        {
            if (!isRunning)
            {
                isRunning = true;

                if (gateway.Connect())
                {
                    log.LogInfo("Successfully connected to gateway");
                    return true;
                }
                else
                    return false;
            }
            else
                throw new InvalidOperationException($"Shard {ShardId} has already been started!");
        }

        internal bool Stop()
        {
            if (isRunning)
            {
                isRunning = false;

                Guilds.Clear();
                Channels.Clear();
                Roles.Clear();
                DirectMessageChannels.Clear();
                Users.Clear();

                return gateway.Disconnect();
            }
            else
                throw new InvalidOperationException($"Shard {ShardId} has already been stopped!");
        }

        public void Dispose()
        {
            gateway.Dispose();
        }
    }
}
