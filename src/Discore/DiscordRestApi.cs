using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore
{
    public class DiscordRestApi
    {
        DiscordApplication app;

        internal DiscordRestApi(DiscordApplication app)
        {
            this.app = app;
        }
    }
}
