namespace Discore
{
    public class DiscordGame : IDiscordObject
    {
        public string Name { get; set; }
        public DiscordGameType Type { get; set; }

        public void Update(DiscordApiData data)
        {
            Name = data.GetString("name") ?? Name;

            int? type = data.GetInteger("type");
            if (type.HasValue)
                Type = (DiscordGameType)type.Value;
        }
    }
}
