[[← back]](./README.md)

# Getting Started
**Note:** If you are migrating from Discore 2.x, please [see the migration guide here](./2x-to-3x-Migration-Guide).

## 1. Check .NET Compatibility
**Discore 3.x targets .NET Standard 1.6.**

See the [.NET Standard documentation](https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-implementation-support) for compatible .NET implementations.

## 2. Acquire Discore

### Download from NuGet
[Releases can be downloaded via NuGet here.](https://www.nuget.org/packages/Discore/)

### Compile from Source
The project can be built with [Visual Studio 2017](https://www.visualstudio.com/downloads/) (e.g. via the ".NET Core cross-platform development" workload).

## 3. (Optional) Try the Example Bot
If you wish to test your Discore installation, try this example bot. Just enter your bot's user token for the `TOKEN` constant and fire away!

```csharp
using Discore;
using Discore.Http;
using Discore.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscorePingPong
{
    public class Program
    {
        DiscordHttpClient http;

        public static void Main(string[] args)
        {
            Program program = new Program();
            program.Run().Wait();
        }

        public async Task Run()
        {
            const string TOKEN = "<bot user token goes here>";

            // Create an HTTP client.
            http = new DiscordHttpClient(TOKEN);

            // Create a single shard.
            using (Shard shard = new Shard(TOKEN, 0, 1))
            {
                // Subscribe to the message creation event.
                shard.Gateway.OnMessageCreated += Gateway_OnMessageCreated;

                // Start the shard.
                await shard.StartAsync();
                Console.WriteLine("Bot started!");

                // Wait for the shard to end before closing the program.
                await shard.WaitUntilStoppedAsync();
            }
        }

        private async void Gateway_OnMessageCreated(object sender, MessageEventArgs e)
        {
            Shard shard = e.Shard;
            DiscordMessage message = e.Message;

            if (message.Author.Id == shard.UserId)
                // Ignore messages created by our bot.
                return;

            if (message.Content == "!ping")
            {
                try
                {
                    // Reply to the user who posted "!ping".
                    await http.CreateMessage(message.ChannelId, $"<@!{message.Author.Id}> Pong!");
                }
                catch (DiscordHttpApiException) { /* Message failed to send... :( */ }
            }
        }
    }
}
```

## 4. (Optional) Check out other examples
Discore has a repository dedicated to sample bots, which [can be found here](https://github.com/BundledSticksInkorperated/Discore.Samples).

## 5. Start building your own bot
All that's left now is to build your bot!

See the following major sections to learn more:
- [HTTP Clients](./HTTP-Clients)
- [Shards](./Shards)