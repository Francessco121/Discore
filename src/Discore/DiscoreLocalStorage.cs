using System.IO;

namespace Discore
{
    class DiscoreLocalStorage
    {
        /// <summary>
        /// Gets the existing or creates a new instance of the local storage API.
        /// </summary>
        public static DiscoreLocalStorage Instance
        {
            get
            {
                if (instance == null)
                    instance = new DiscoreLocalStorage();

                return instance;
            }
        }

        static DiscoreLocalStorage instance;

        public string GatewayUrl
        {
            get { return data.GetString("gateway_url"); }
            set { data.Set("gateway_url", value); }
        }

        const string FILE_NAME = "discore-local-storage.json";

        DiscordApiData data;
        DiscoreLogger log;

        private DiscoreLocalStorage()
        {
            log = new DiscoreLogger("DiscoreLocalStorage");

            if (File.Exists(FILE_NAME))
            {
                using (FileStream fs = File.Open(FILE_NAME, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader reader = new StreamReader(fs))
                {
                    string json = reader.ReadToEnd();

                    if (!DiscordApiData.TryParseJson(json, out data))
                    {
                        log.LogWarning($"{FILE_NAME} contained invalid JSON, overwriting with a new file.");
                        CreateNewFile();
                    }
                }
            }
            else
                CreateNewFile();
        }

        void CreateNewFile()
        {
            // Save empty JSON for now.
            data = new DiscordApiData();
            Save();
        }

        public void Save()
        {
            using (FileStream fs = File.Open(FILE_NAME, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                string json = data.SerializeToJson();
                writer.Write(json);
            }
        }
    }
}
