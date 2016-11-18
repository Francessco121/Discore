using Discore;
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
            DiscordBotUserAuthenticator auth = new DiscordBotUserAuthenticator("");
            DiscordWebSocketApplication app = new DiscordWebSocketApplication(auth);
            DiscordApiData data = app.InternalHttpApi.Gateway.Get();

            TestHelper.Assert(data != null, "Response should not return null");
            TestHelper.Assert(data.ContainsKey("url"), "Response did not contain expected contents.");
        }
    }
}
