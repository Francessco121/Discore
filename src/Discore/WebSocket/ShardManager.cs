using Discore.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket
{
    public class ShardManager : IDisposable
    {
        /// <summary>
        /// Gets the number of shards currently being managed.
        /// </summary>
        public int ManagedShardCount => shards?.Length ?? 0;
        /// <summary>
        /// Gets the total number of shards currently used by the Discord application.
        /// <para>This can be more than the number of managed shards, as other processes could be handling some shards.</para>
        /// </summary>
        public int TotalShardCount { get; private set; }
        /// <summary>
        /// Gets a list of all shards currently being managed,
        /// or null if no shards have been created yet.
        /// </summary>
        public IReadOnlyList<Shard> Shards => shards;

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
        /// </summary>
        /// <returns>Returns the created shard.</returns>
        /// <exception cref="InvalidOperationException">Thrown if any existing shard is still running.</exception>
        public Shard CreateSingleShard()
        {
            CreateShards(new int[] { 0 });
            return shards[0];
        }

        /// <summary>
        /// Creates the minimum number of shards required by the Discord application.
        /// This number is specified by the Discord API.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if any existing shard is still running.</exception>
        [Obsolete("Please use the asynchronous counterpart CreateMinimumRequiredShardsAsync() instead.")]
        public void CreateMinimumRequiredShards()
        {
            CreateMinimumRequiredShardsAsync().Wait();
        }

        /// <summary>
        /// Creates the minimum number of shards required by the Discord application.
        /// This number is specified by the Discord API.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if any existing shard is still running.</exception>
        public async Task CreateMinimumRequiredShardsAsync()
        {
            int numShards;

            GatewayBotResponse response = await app.HttpApi.Gateway.GetBot().ConfigureAwait(false);
            numShards = response.Shards;

            // GET /gateway/bot also specifies the gateway url, update local storage
            // with this value if it differs so we don't need to call GET /gateway
            // later on when connecting.
            string gatewayUrl = response.Url;
            DiscoreLocalStorage localStorage = await DiscoreLocalStorage.GetInstanceAsync().ConfigureAwait(false);
            if (localStorage.GatewayUrl != gatewayUrl)
            {
                localStorage.GatewayUrl = gatewayUrl;
                await localStorage.SaveAsync().ConfigureAwait(false);
            }

            // Create the minimum shards specified by the Discord API.
            CreateShards(numShards);
        }

        /// <summary>
        /// Creates the specified number of shards, where the shard ids range
        /// from 0 to the number specified - 1.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the number of shards specified is less than 1.</exception>
        /// <exception cref="InvalidOperationException">Thrown if any existing shard is still running.</exception>
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
        /// <exception cref="InvalidOperationException">Thrown if any existing shard is still running.</exception>
        public void CreateShards(ICollection<int> shardIds, int? totalShards = null)
        {
            if (shardIds == null)
                throw new ArgumentNullException(nameof(shardIds));
            if (shardIds.Count == 0)
                throw new ArgumentException("At least one shard must be specified.", nameof(shardIds));
            if (totalShards.HasValue && totalShards.Value < shardIds.Count)
                throw new ArgumentOutOfRangeException(nameof(totalShards),
                    "Number of total shards must be greater than or equal to the number of shard ids specified.");
            if (IsAnyShardRunning())
                throw new InvalidOperationException("Cannot create new shards until all previous have been stopped.");

            // Cleanup existing shards
            if (shards != null)
                DisposeShards();

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

        bool IsAnyShardRunning()
        {
            if (shards != null)
            {
                for (int i = 0; i < shards.Length; i++)
                    if (shards[i].IsRunning)
                        return true;
            }

            return false;
        }

        /// <summary>
        /// Starts all created shards that are not running.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if no shards were created prior.</exception>
        [Obsolete("Please use the asynchronous counterpart StartShardsAsync(CancellationToken) instead.")]
        public void StartShards()
        {
            StartShardsAsync(CancellationToken.None);
        }

        /// <summary>
        /// Starts all created shards that are not running, and returns a list of tasks representing each startup.
        /// These tasks will not finish until their respected shard has successfully connected (or is canceled).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if no shards were created prior.</exception>
        public IList<Task> StartShardsAsync(CancellationToken cancellationToken)
        {
            if (shards != null)
            {
                List<Task> startTasks = new List<Task>();
                for (int i = 0; i < shards.Length; i++)
                {
                    Shard shard = shards[i];
                    if (shard.IsRunning)
                        continue;

                    startTasks.Add(shard.StartAsync(cancellationToken));
                }

                return startTasks;
            }
            else
                throw new InvalidOperationException("No shards have been created.");
        }

        /// <summary>
        /// Stops all created shards that are running.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if no shards were created prior.</exception>
        [Obsolete("Please use the asynchronous counterpart StopShardsAsync() instead.")]
        public void StopShards()
        {
            Task.WhenAll(StopShardsAsync()).Wait();
        }

        /// <summary>
        /// Stops all created shards that are running, and returns a list of tasks representing each disconnection.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if no shards were created prior.</exception>
        public IList<Task> StopShardsAsync()
        {
            if (shards != null)
            {
                List<Task> stopTasks = new List<Task>();
                for (int i = 0; i < shards.Length; i++)
                {
                    Shard shard = shards[i];
                    if (!shard.IsRunning)
                        continue;

                    stopTasks.Add(shards[i].StopAsync());
                }

                return stopTasks;
            }
            else
                throw new InvalidOperationException("No shards have been created.");
        }

        void DisposeShards()
        {
            for (int i = 0; i < shards.Length; i++)
                shards[i].Dispose();
        }

        public void Dispose()
        {
            if (shards != null)
                DisposeShards();
        }
    }
}
