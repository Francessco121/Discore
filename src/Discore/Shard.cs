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

        internal Shard(DiscordApplication app, int shardId)
        {
            Application = app;
            ShardId = shardId;

            Cache = new DiscordApiCache();
            gateway = new Gateway();
        }

        internal void Start()
        {
            if (!isRunning)
            {
                isRunning = true;

                gateway.Connect();
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
