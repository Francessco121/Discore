using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore.Http.Net
{
    class DiscordRestEndpoint
    {
        internal RestClient Rest { get; }

        internal DiscordRestEndpoint(RestClient restClient)
        {
            Rest = restClient;
        }
    }
}
