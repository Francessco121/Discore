using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore.Net.Rest
{
    public class DiscordRestApi
    {
        public DiscordRestGatewayEndpoint Gateway { get; }

        DiscordApplication app;
        RestClient client;

        internal DiscordRestApi(DiscordApplication app)
        {
            this.app = app;

            client = new RestClient(app);

            Gateway = new DiscordRestGatewayEndpoint(client);
        }
    }
}
