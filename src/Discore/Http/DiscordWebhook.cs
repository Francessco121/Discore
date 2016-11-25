using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore.Http
{
    public class DiscordWebhook : DiscordIdObject
    {
        public Snowflake Guild { get; }
        public Snowflake Channel { get; }
        public DiscordUser User { get; }
        public string Name { get; }
        public DiscordAvatarData Avatar { get; }
        public string Token { get; }

        public DiscordWebhook(DiscordApiData data)
            :base(data)
        {
            Guild = data.GetSnowflake("guild_id").Value;
            Channel = data.GetSnowflake("channel_id").Value;

            DiscordApiData tData;
            if (DiscordApiData.TryParseJson(data.GetString("user"), out tData))
                //User = new DiscordUser(tData);

            Name = data.GetString("name");
            Avatar = new DiscordAvatarData(data.GetString("avatar"));
            Token = data.GetString("token");
        }
    }
}
