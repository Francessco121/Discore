namespace Discore
{
    static class Utils
    {
        public static DiscordUserStatus? ParseUserStatus(string str)
        {
            switch (str)
            {
                case "offline":
                    return DiscordUserStatus.Offline;
                case "invisible":
                    return DiscordUserStatus.Invisible;
                case "dnd":
                    return DiscordUserStatus.DoNotDisturb;
                case "idle":
                    return DiscordUserStatus.Idle;
                case "online":
                    return DiscordUserStatus.Online;
                default:
                    return null;
            }
        }
    }
}
