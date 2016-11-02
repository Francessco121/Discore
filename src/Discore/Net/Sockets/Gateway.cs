using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Discore.Net.Sockets
{
    class Gateway
    {
        DiscoreWebSocket socket;

        internal Gateway()
        {
            socket = new DiscoreWebSocket("Gateway");
            socket.OnConnected += Socket_OnConnected;
            socket.OnDisconnected += Socket_OnDisconnected;
            socket.OnError += Socket_OnError;
            socket.OnMessageReceived += Socket_OnMessageReceived;
        }

        public bool Connect()
        {
            return socket.Connect($"{"wss://gateway.discord.gg/"}/encoding=json&v=5");
        }

        private void Socket_OnConnected(object sender, Uri e)
        {
            
        }

        private void Socket_OnDisconnected(object sender, WebSocketCloseStatus e)
        {
            
        }

        private void Socket_OnError(object sender, Exception e)
        {
            
        }

        private void Socket_OnMessageReceived(object sender, DiscordApiData e)
        {
            
        }
    }
}
