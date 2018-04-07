using Discore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Discore
{
    class DiscoreLocalStorage
    {
        static DiscoreLocalStorage instance;

        string GatewayUrl
        {
            get => data.GetString("gateway_url");
            set => data.Set("gateway_url", value);
        }

        const string FILE_NAME = "discore-local-storage.json";

        DiscordApiData data;
        DiscoreLogger log;

        private DiscoreLocalStorage()
        {
            log = new DiscoreLogger("DiscoreLocalStorage");
        }

        /// <summary>
        /// Gets the existing or creates a new instance of the local storage API.
        /// </summary>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public static async Task<DiscoreLocalStorage> GetInstanceAsync()
        {
            if (instance == null)
            {
                instance = new DiscoreLocalStorage();
                await instance.OpenAsync().ConfigureAwait(false);
            }

            return instance;
        }

        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        async Task OpenAsync()
        {
            if (File.Exists(FILE_NAME))
            {
                using (FileStream fs = File.Open(FILE_NAME, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader reader = new StreamReader(fs))
                {
                    string json = await reader.ReadToEndAsync().ConfigureAwait(false);

                    if (!DiscordApiData.TryParseJson(json, out data))
                    {
                        log.LogWarning($"{FILE_NAME} contained invalid JSON, overwriting with a new file...");
                        await CreateNewFileAsync().ConfigureAwait(false);
                    }
                }
            }
            else
                await CreateNewFileAsync().ConfigureAwait(false);
        }

        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        async Task CreateNewFileAsync()
        {
            // Save empty JSON for now.
            data = new DiscordApiData();
            await SaveAsync().ConfigureAwait(false);
        }

        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        async Task SaveAsync()
        {
            using (FileStream fs = File.Open(FILE_NAME, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                string json = data.SerializeToJson();
                await writer.WriteAsync(json).ConfigureAwait(false);
            }
        }

        /// <param name="useCached">
        /// Whether to use the cached gateway URL (if available) or to pull down a new one via HTTP.
        /// </param>
        /// <exception cref="DiscordHttpApiException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public async Task<string> GetGatewayUrlAsync(DiscordHttpClient http, bool useCached = true)
        {
            if (string.IsNullOrWhiteSpace(GatewayUrl) || !useCached)
            {
                log.LogVerbose("Retrieving gateway URL from HTTP...");

                string gatewayUrl = await http.Get().ConfigureAwait(false);

                if (GatewayUrl != gatewayUrl)
                {
                    GatewayUrl = gatewayUrl;
                    await SaveAsync().ConfigureAwait(false);
                }

                return gatewayUrl;
            }
            else
                return GatewayUrl;
        }

        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        public Task InvalidateGatewayUrlAsync()
        {
            if (GatewayUrl != null)
            {
                log.LogVerbose("Invalidating gateway URL...");

                GatewayUrl = null;
                return SaveAsync();
            }
            else
                return Task.CompletedTask;
        }
    }
}
