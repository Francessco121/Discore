using Discore;
using Discore.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscoreTest.Tests
{
    [TestClass]
    public class RestClientTest
    {
        [TestMethod]
        public static void GetGatewayTest()
        {
            DiscordBotUserToken auth = new DiscordBotUserToken("");
            DiscordWebSocketApplication app = new DiscordWebSocketApplication(auth);
            string gatewayUrl = app.HttpApi.Gateway.Get().Result;

            TestHelper.Assert(gatewayUrl != null, "Response should not return null");
        }
    }
}
