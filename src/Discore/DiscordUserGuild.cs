namespace Discore
{
    public class DiscordUserGuild : IDiscordObject, ICacheable
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Icon { get; private set; }
        public bool Owner { get; private set; }
        public int Permissions { get; private set; }

        public void Update(DiscordApiData data)
        {
            Id          = data.GetString("id") ?? Id;
            Name        = data.GetString("name") ?? Name;
            Icon        = data.GetString("icon") ?? Icon;
            Owner       = data.GetBoolean("owner") ?? Owner;
            Permissions = data.GetInteger("permissions") ?? Permissions;
        }
    }
}
