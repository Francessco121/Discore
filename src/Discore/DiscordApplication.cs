using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore
{
    public class DiscordApplication
    {
        public string Token { get; }
        public ShardManager Shards { get; }
        public DiscordRestApi Rest { get; }

        public DiscordApplication(string token)
        {
            Token = token;

            Shards = new ShardManager(this);
            Rest = new DiscordRestApi(this);
        }
    }
}
