# Discore
[![NuGet](https://img.shields.io/nuget/v/Discore.svg?style=flat-square)](https://www.nuget.org/packages/Discore/)

Discore provides a [.NET Standard](https://docs.microsoft.com/en-us/dotnet/articles/standard/library) interface with [Discord's](https://discordapp.com/) HTTP, Gateway, and voice APIs. 

**Please note!** Discore is **not** an official Discord API wrapper.

Released under the [MIT License](../master/LICENSE.md).

## Downloading
Official releases are available through [NuGet](https://www.nuget.org/packages/Discore/). These are published alongside a [GitHub release](https://github.com/BundledSticksInkorperated/Discore/releases), which contains the full change log.

## Compiling
Discore currently targets .NET Standard 1.6.

The project can be built with [Visual Studio 2017](https://www.visualstudio.com/downloads/) or any other .NET build system that can target .NET Standard 1.6.

## Wiki
For more information on how to use Discore, see our [documentation right here on GitHub](https://github.com/BundledSticksInkorperated/Discore/wiki).

## Example Bot: Ping Pong
```csharp
using Discore;
using Discore.WebSocket;
using System;
using System.Threading;
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
            await shard.StartAsync(CancellationToken.None);

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
                    await textChannel.SendMessage($"<@{message.Author.Id}> Pong!");
                }
                catch (Exception) {  /* Message failed to send... :( */ }
            }
        }
    }
}
```

## Alternatives
- [Discord.Net](https://github.com/RogueException/Discord.Net)
- [DSharpPlus](https://github.com/NaamloosDT/DSharpPlus)
