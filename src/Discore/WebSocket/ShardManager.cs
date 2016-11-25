using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Discore.WebSocket
{
    public class ShardManager
    {
        public int ShardCount { get { return shards?.Length ?? 0; } }
        public IReadOnlyList<Shard> Shards { get { return new ReadOnlyCollection<Shard>(shards); } }

        DiscordWebSocketApplication app;
        Shard[] shards;

        internal ShardManager(DiscordWebSocketApplication app)
        {
            this.app = app;
        }

        public void CreateShards(int numberOfShards = 1)
        {
            if (numberOfShards < 1)
                throw new ArgumentOutOfRangeException("numberOfShards", "numberOfShards must be above or equal to 1");

            // Stop existing shards
            ShutdownShards();

            // Create new shards
            shards = new Shard[numberOfShards];
            for (int i = 0; i < numberOfShards; i++)
            {
                Shard shard = new Shard(app, i);
                shards[i] = shard;
            }
        }

        public bool StartShards()
        {
            if (shards != null)
            {
                for (int i = 0; i < shards.Length; i++)
                {
                    if (!shards[i].Start())
                        return false;
                }

                return true;
            }
            else
                return false;
        }

        public void ShutdownShards()
        {
            if (shards != null)
            {
                for (int i = 0; i < shards.Length; i++)
                    shards[i].Stop();

                shards = null;
            }
        }
    }
}
