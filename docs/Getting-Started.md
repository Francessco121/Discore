[[← back]](./README.md)

# Getting Started
## 1. Acquire Discore

### Pre-built Downloads
[Pre-built releases can be downloaded via NuGet here.](https://www.nuget.org/packages/Discore/)

### Compile from Source
The project can be built with [Visual Studio 2017](https://www.visualstudio.com/downloads/) (e.g. via the ".NET Core 1.0 - 1.1 development tools").

## 2. Choose your type of Discord application
Applications using Discore are not required to use all three of the APIs. Instead, applcations only need to use the APIs they need as well as the dependencies of those APIs. For example: applications that just need the HTTP API require nothing else, while applications using the voice API require a Gateway connection, and a Gateway connection requires the HTTP API:

HTTP < Gateway < Voice

- [Voice or Gateway usage](./WebSocket-Applications.md).
- [HTTP only](./HTTP-Applications.md).

## Example Bot: Ping Pong
If you wish to test your Discore installation, try this example bot. Just enter your bot's user token for the `token` variable and fire away!

```csharp
using Discore;
using Discore.Http;
using Discore.WebSocket;
using System.Threading.Tasks;

namespace DiscorePingPong
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Program program = new Program();
            program.Run().Wait();
        }

        public async Task Run()
        {
            // Create authenticator using a bot user token.
            DiscordBotUserToken token = new DiscordBotUserToken("<bot user token goes here>");

            // Create a WebSocket application.
            DiscordWebSocketApplication app = new DiscordWebSocketApplication(token);

            // Create and start a single shard.
            Shard shard = app.ShardManager.CreateSingleShard();
            await shard.StartAsync();

            // Subscribe to the message creation event.
            shard.Gateway.OnMessageCreated += Gateway_OnMessageCreated;

            // Wait for the shard to end before closing the program.
            while (shard.IsRunning)
                await Task.Delay(1000);
        }

        private static async void Gateway_OnMessageCreated(object sender, MessageEventArgs e)
        {
            Shard shard = e.Shard;
            DiscordMessage message = e.Message;

            if (message.Author == shard.User)
                // Ignore messages created by our bot.
                return;

            if (message.Content == "!ping")
            {
                // Grab the DM or guild text channel this message was posted in from cache.
                ITextChannel textChannel = (ITextChannel)shard.Cache.Channels.Get(message.ChannelId);

                try
                {
                    // Reply to the user who posted "!ping".
                    await textChannel.CreateMessage($"<@!{message.Author.Id}> Pong!");
                }
                catch (DiscordHttpApiException) { /* Message failed to send... :( */ }
            }
        }
    }
}
```