using System.IO;
using System.Threading.Tasks;

namespace Discore
{
    class DiscoreLocalStorage
    {
        static DiscoreLocalStorage instance;

        public string GatewayUrl
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
        public static async Task<DiscoreLocalStorage> GetInstanceAsync()
        {
            if (instance == null)
            {
                instance = new DiscoreLocalStorage();
                await instance.OpenAsync().ConfigureAwait(false);
            }

            return instance;
        }

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
                        log.LogWarning($"{FILE_NAME} contained invalid JSON, overwriting with a new file.");
                        await CreateNewFileAsync().ConfigureAwait(false);
                    }
                }
            }
            else
                await CreateNewFileAsync().ConfigureAwait(false);
        }

        async Task CreateNewFileAsync()
        {
            // Save empty JSON for now.
            data = new DiscordApiData();
            await SaveAsync().ConfigureAwait(false);
        }

        public async Task SaveAsync()
        {
            using (FileStream fs = File.Open(FILE_NAME, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                string json = data.SerializeToJson();
                await writer.WriteAsync(json).ConfigureAwait(false);
            }
        }
    }
}
