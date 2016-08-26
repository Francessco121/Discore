using System.IO;

namespace Discore
{
    class DiscordApiInfoCache
    {
        const string FILE_PATH = "discord-api-info.json";

        public string GatewayEndpoint
        {
            get { return data.GetString("gateway"); }
            set { data.Set("gateway", value); }
        }

        DiscordApiData data;

        public DiscordApiInfoCache()
        {
            LoadOrCreate();
        }

        void LoadOrCreate()
        {
            using (FileStream fs = File.Open(FILE_PATH, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            using (StreamReader reader = new StreamReader(fs))
            {
                string jsonStr = reader.ReadToEnd();

                if (string.IsNullOrWhiteSpace(jsonStr))
                    data = new DiscordApiData();
                else
                    data = DiscordApiData.FromJson(jsonStr);
            }
        }

        public void Save()
        {
            using (FileStream fs = File.Open(FILE_PATH, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            using (StreamWriter writer = new StreamWriter(fs))
            {
                string jsonStr = data.SerializeToJson();
                writer.Write(jsonStr);
            }
        }
    }
}
