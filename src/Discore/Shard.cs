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

        DiscordApplication app;
        bool isRunning;

        internal Shard(DiscordApplication app, int shardId)
        {
            this.app = app;
            ShardId = shardId;

            Cache = new DiscordApiCache();
        }

        internal void Start()
        {
            if (!isRunning)
            {
                isRunning = true;

                // TODO: start
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
