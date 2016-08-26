namespace Discore
{
    public class DiscordRole : IDiscordObject, ICacheable
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public DiscordColor Color { get; private set; }
        public bool IsHoisted { get; private set; }
        public int Position { get; private set; }
        public DiscordPermission Permissions { get; private set; }
        public bool IsManaged { get; private set; }
        public bool IsMentionable { get; private set; }

        public void Update(DiscordApiData data)
        {
            Id              = data.GetString("id") ?? Id;
            Name            = data.GetString("name") ?? Name;
            IsHoisted       = data.GetBoolean("hoist") ?? IsHoisted;
            Position        = data.GetInteger("position") ?? Position;
            IsManaged       = data.GetBoolean("managed") ?? IsManaged;
            IsMentionable   = data.GetBoolean("mentionable") ?? IsMentionable;

            int? color = data.GetInteger("color");
            if (color.HasValue)
                Color = DiscordColor.FromHexadecimal(color.Value);

            long? permissions = data.GetInt64("permissions");
            if (permissions.HasValue)
                Permissions = (DiscordPermission)permissions.Value;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
