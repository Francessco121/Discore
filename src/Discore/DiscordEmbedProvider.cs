namespace Discore
{
    public class DiscordEmbedProvider : IDiscordObject
    {
        public string Name { get; private set; }
        public string Url { get; private set; }

        public void Update(DiscordApiData data)
        {
            Name = data.GetString("name") ?? Name;
            Url = data.GetString("url") ?? Url;
        }
    }
}
