namespace Discore
{
    static class Utils
    {
        public static DiscordUserStatus? ParseUserStatus(string str)
        {
            return str switch
            {
                "offline" => DiscordUserStatus.Offline,
                "invisible" => DiscordUserStatus.Invisible,
                "dnd" => DiscordUserStatus.DoNotDisturb,
                "idle" => DiscordUserStatus.Idle,
                "online" => DiscordUserStatus.Online,
                _ => null,
            };
        }

        public static string? UserStatusToString(DiscordUserStatus status)
        {
            return status switch
            {
                DiscordUserStatus.Offline => "offline",
                DiscordUserStatus.Invisible => "invisible",
                DiscordUserStatus.DoNotDisturb => "dnd",
                DiscordUserStatus.Idle => "idle",
                DiscordUserStatus.Online => "online",
                _ => null,
            };
        }
    }
}
