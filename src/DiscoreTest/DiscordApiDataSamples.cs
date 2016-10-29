using Discore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoreTest
{
    public static class DiscordApiDataSamples
    {
        public static DiscordApiData GetDiscordUser()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("id", "356783458673456");
            data.Set("username", "TestUser");
            data.Set("discriminator", "0000");
            data.Set("avatar", "");
            data.Set("mfa_enabled", false);

            return data;
        }

        public static DiscordApiData GetDiscordGuildMember()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("user", GetDiscordUser());
            data.Set("nick", "TestNickname");
            data.Set("deaf", false);
            data.Set("mute", true);
            data.Set("joined_at", DateTime.Now);

            return data;
        }
    }
}
