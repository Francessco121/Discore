namespace Discore
{
    public class DiscordOverwrite : IDiscordObject, ICacheable
    {
        public string Id { get; private set; }
        public DiscordOverwriteType Type { get; private set; }
        public DiscordPermission Allow { get; private set; }
        public DiscordPermission Deny { get; private set; }

        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;

            string type = data.GetString("type");
            if (type != null)
            {
                switch (type)
                {
                    case "role":
                        Type = DiscordOverwriteType.Role;
                        break;
                    case "member":
                        Type = DiscordOverwriteType.Member;
                        break;
                }
            }

            long? allow = data.GetInt64("allow");
            if (allow.HasValue)
                Allow = (DiscordPermission)allow.Value;

            long? deny = data.GetInt64("deny");
            if (deny.HasValue)
                Deny = (DiscordPermission)deny.Value;
        }
    }
}
