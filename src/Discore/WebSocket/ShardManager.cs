using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Discore.WebSocket
{
    public class ShardManager : IDisposable
    {
        /// <summary>
        /// Gets the number of shards currently being managed.
        /// </summary>
        public int ShardCount { get { return shards?.Length ?? 0; } }
        /// <summary>
        /// Gets a list of all shards currently being managed.
        /// </summary>
        public IReadOnlyList<Shard> Shards { get { return new ReadOnlyCollection<Shard>(shards); } }

        DiscordWebSocketApplication app;
        Shard[] shards;

        internal ShardManager(DiscordWebSocketApplication app)
        {
            this.app = app;
        }

        /// <summary>
        /// Creates a single shard to be managed.
        /// This is useful if you know your application only requires one shard.
        /// </summary>
        /// <returns>Returns the created shard.</returns>
        public Shard CreateSingleShard()
        {
            CreateShards(new int[] { 0 });
            return shards[0];
        }

        /// <summary>
        /// Creates the specified number of shards, where the shard ids range
        /// from 0 to the number specified - 1.
        /// </summary>
        public void CreateShards(int numberOfShards = 1)
        {
            if (numberOfShards < 1)
                throw new ArgumentOutOfRangeException(nameof(numberOfShards), "Number of shards must be above or equal to 1");

            int[] shardIds = new int[numberOfShards];
            for (int i = 0; i < numberOfShards; i++)
                shardIds[i] = i;

            CreateShards(shardIds);
        }

        /// <summary>
        /// Creates shards for each shard id specified.
        /// </summary>
        public void CreateShards(ICollection<int> shardIds)
        {
            if (shardIds == null)
                throw new ArgumentNullException(nameof(shardIds));
            if (shardIds.Count == 0)
                throw new ArgumentException("At least one shard must be specified.", nameof(shardIds));

            // Stop existing shards
            ShutdownShards();

            // Create new shards
            shards = new Shard[shardIds.Count];
            int i = 0;
            foreach (int id in shardIds)
            {
                Shard shard = new Shard(app, id);
                shards[i++] = shard;
            }
        }

        /// <summary>
        /// Attempts to start all created shards that are currently not running.
        /// <para>
        /// If this returns false, there either were no shards to start, or one of them failed.
        /// </para>
        /// </summary>
        /// <returns>Returns whether every shard was successfully started.</returns>
        public bool StartShards()
        {
            if (shards != null)
            {
                bool allSucceeded = true;
                for (int i = 0; i < shards.Length; i++)
                {
                    Shard shard = shards[i];
                    if (shard.IsRunning)
                        continue;

                    if (!shard.Start())
                        allSucceeded = false;
                }

                return allSucceeded;
            }
            else
                return false;
        }

        /// <summary>
        /// Attempts to stop all created shards that are still running.
        /// <para>
        /// If this returns false, there either were no shards to stop, or one of them failed to disconnect.
        /// </para>
        /// </summary>
        /// <returns>Returns whether every shard was successfully stopped.</returns>
        public bool ShutdownShards()
        {
            if (shards != null)
            {
                bool allDisconnected = true;
                for (int i = 0; i < shards.Length; i++)
                {
                    Shard shard = shards[i];
                    if (!shard.IsRunning)
                        continue;

                    if (!shards[i].Stop())
                        allDisconnected = false;
                }

                shards = null;
                return allDisconnected;
            }
            else
                return false;
        }

        public void Dispose()
        {
            if (shards != null)
            {
                for (int i = 0; i < shards.Length; i++)
                    shards[i].Dispose();
            }
        }
    }
}
