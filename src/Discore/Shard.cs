using Discore.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore
{
    public class Shard
    {
        public int ShardId { get; }
        public DiscordApiCache Cache { get; }
        public DiscordApplication Application { get; }

        bool isRunning;
        Gateway gateway;
        DiscoreLogger log;

        internal Shard(DiscordApplication app, int shardId)
        {
            Application = app;
            ShardId = shardId;

            log = new DiscoreLogger($"Shard {shardId}");

            Cache = new DiscordApiCache();
            gateway = new Gateway(app, this);
        }

        internal void Start()
        {
            if (!isRunning)
            {
                isRunning = true;

                if (gateway.Connect())
                {
                    log.LogInfo("Successfully connected to gateway");
                }
            }
            else
                throw new InvalidOperationException($"Shard {ShardId} has already been started!");
        }

        internal void Stop()
        {
            if (isRunning)
            {
                isRunning = false;

                Cache.Clear();

                // TODO: shutdown
            }
            else
                throw new InvalidOperationException($"Shard {ShardId} has already been stopped!");
        }
    }
}
