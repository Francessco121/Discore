using Discore.Http.Net;
using Discore.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore
{
    public class DiscordApplication
    {
        public string Token { get; }
        public ShardManager ShardManager { get; }

        internal DiscordRestApi Rest { get; }

        public DiscordApplication(string token)
        {
            Token = token;

            ShardManager = new ShardManager(this);
            Rest = new DiscordRestApi(this);
        }
    }
}
