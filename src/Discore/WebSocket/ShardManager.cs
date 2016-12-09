using System;
using System.Collections.Generic;

namespace Discore.WebSocket
{
    public class ShardManager : IDisposable
    {
        /// <summary>
        /// Gets the number of shards currently being managed.
        /// </summary>
        public int ManagedShardCount { get { return shards?.Length ?? 0; } }
        /// <summary>
        /// Gets the total number of shards currently used by the Discord application.
        /// <para>This can be more than the number of managed shards, as other processes could be handling some shards.</para>
        /// </summary>
        public int TotalShardCount { get; private set; }
        /// <summary>
        /// Gets a list of all shards currently being managed,
        /// or null if no shards have been created yet.
        /// </summary>
        public IReadOnlyList<Shard> Shards { get { return shards; } }

        DiscordWebSocketApplication app;
        DiscoreLogger log;
        Shard[] shards;

        internal ShardManager(DiscordWebSocketApplication app)
        {
            this.app = app;

            log = new DiscoreLogger("ShardManager");
        }

        /// <summary>
        /// Creates a single shard to be managed.
        /// This is useful if you know your application only requires one shard, or for testing.
        /// <para>Will shutdown existing shards.</para>
        /// </summary>
        /// <returns>Returns the created shard.</returns>
        public Shard CreateSingleShard()
        {
            CreateShards(new int[] { 0 });
            return shards[0];
        }

        /// <summary>
        /// Creates the minimum number of shards required by the Discord application.
        /// <para>Will shutdown existing shards.</para>
        /// </summary>
        public void CreateMinimumRequiredShards()
        {
            int numShards;
            try
            {
                DiscordApiData data = app.HttpApi.InternalApi.Gateway.GetBot().Result;
                numShards = data.GetInteger("shards").Value;

                // GET /gateway/bot also specifies the gateway url, update local storage
                // with this value if it differs so we don't need to call GET /gateway
                // later on when connecting.
                string gatewayUrl = data.GetString("url");
                DiscoreLocalStorage localStorage = DiscoreLocalStorage.Instance;
                if (localStorage.GatewayUrl != gatewayUrl)
                {
                    localStorage.GatewayUrl = gatewayUrl;
                    localStorage.Save();
                }
            }
            catch (AggregateException aex) { throw aex.InnerException; }

            // Create the minimum shards specified by the Discord API.
            CreateShards(numShards);
        }

        /// <summary>
        /// Creates the specified number of shards, where the shard ids range
        /// from 0 to the number specified - 1.
        /// <para>Will shutdown existing shards.</para>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the number of shards specified is less than 1.</exception>
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
        /// <para>Will shutdown existing shards.</para>
        /// </summary>
        /// <param name="shardIds">A collection of shard ids to be managed by this process.</param>
        /// <param name="totalShards">
        /// The total number of shards for the Discord application.
        /// If null, equals the number of shard ids specified by <paramref name="shardIds"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="shardIds"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if no shard ids are specified.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="totalShards"/> is specified, 
        /// but it is less than the number of shard ids specified.
        /// </exception>
        public void CreateShards(ICollection<int> shardIds, int? totalShards = null)
        {
            if (shardIds == null)
                throw new ArgumentNullException(nameof(shardIds));
            if (shardIds.Count == 0)
                throw new ArgumentException("At least one shard must be specified.", nameof(shardIds));
            if (totalShards.HasValue && totalShards.Value < shardIds.Count)
                throw new ArgumentOutOfRangeException(nameof(totalShards),
                    "Number of total shards must be greater than or equal to the number of shard ids specified.");

            // Stop existing shards
            if (shards != null)
                ShutdownShards();

            // Set total shard count
            TotalShardCount = totalShards ?? shardIds.Count;

            // Create new shards
            shards = new Shard[shardIds.Count];
            int i = 0;
            foreach (int id in shardIds)
            {
                Shard shard = new Shard(app, id);
                shards[i++] = shard;
            }

            log.LogInfo($"Created {shardIds.Count} managed shard(s), out of {TotalShardCount} total.");
        }

        /// <summary>
        /// Attempts to start all created shards that are currently not running.
        /// <para>
        /// This returns false if at least one shard failed to start.
        /// </para>
        /// </summary>
        /// <returns>Returns whether every shard was successfully started.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no shards were created prior.</exception>
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
                throw new InvalidOperationException("No shards have been created.");
        }

        /// <summary>
        /// Attempts to stop all created shards that are still running.
        /// <para>
        /// Retirns false if at least one shard failed to stop.
        /// </para>
        /// </summary>
        /// <returns>Returns whether every shard was successfully stopped.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no shards were created prior.</exception>
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
                throw new InvalidOperationException("No shards have been created.");
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
