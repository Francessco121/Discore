namespace Discore.Audio
{
    public class DiscordVoiceRegion : IDiscordObject, ICacheable
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string SampleHostname { get; private set; }
        public int SamplePort { get; private set; }
        public bool VIPOnly { get; private set; }
        public bool Optimal { get; private set; }

        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Name = data.GetString("name") ?? Name;
            SampleHostname = data.GetString("sample_hostname") ?? SampleHostname;
            SamplePort = data.GetInteger("sample_port") ?? SamplePort;
            VIPOnly = data.GetBoolean("vip") ?? VIPOnly;
            Optimal = data.GetBoolean("optimal") ?? Optimal;
        }
    }
}
