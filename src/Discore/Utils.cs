#nullable enable

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
    }
}

#nullable restore
