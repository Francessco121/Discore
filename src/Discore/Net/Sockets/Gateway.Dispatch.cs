using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore.Net.Sockets
{
    partial class Gateway
    {
        delegate void DispatchCallback(DiscordApiData data);

        Dictionary<string, DispatchCallback> dispatchHandlers;

        void InitializeDispatchHandlers()
        {
            dispatchHandlers = new Dictionary<string, DispatchCallback>();
            
        }
    }
}
