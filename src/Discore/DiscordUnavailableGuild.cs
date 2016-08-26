namespace Discore
{
    public class DiscordUnavailableGuild : IDiscordObject, ICacheable
    {
        public string Id { get; private set; }
        public bool Unavailable { get; private set; }

        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Unavailable = data.GetBoolean("unavailable") ?? Unavailable;
        }
    }
}
