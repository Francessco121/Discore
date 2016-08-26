namespace Discore
{
    public class DiscordIntegrationAccount : IDiscordObject, ICacheable
    {
        public string Id { get; private set; }
        public string Name { get; private set; }

        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Name = data.GetString("name") ?? Name;
        }
    }
}
