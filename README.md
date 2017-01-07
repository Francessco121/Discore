# Discore
[![NuGet](https://img.shields.io/nuget/v/Discore.svg?style=flat-square?maxAge=2592000)](https://www.nuget.org/packages/Discore/)

***Discord + .NET Core = Discore***

A [.NET Core](https://dotnet.github.io/) library for interacting with the [Discord](https://discordapp.com/) API.

Discore aims to fully implement two sides of the Discord API: the HTTP API, and the realtime WebSocket API. It's designed for creating Discord bot applications, as well as applications that do not require realtime data.

**Please note!** Discore is **not** an official Discord API wrapper.

Released under the [MIT License](../master/LICENSE.md).

## NuGet
Each release is available through our [NuGet packages](https://www.nuget.org/packages/Discore/).

## Compiling
Requirements:
- Visual Studio 2015 with Update 3
- .NET Core 1.0.1 tools Preview 2

[Which can be obtained here](https://www.microsoft.com/net/core#windowsvs2015).

OR
- [Visual Studio 2017 RC](https://www.microsoft.com/net/core#windowsvs2017) with the ".NET Core and Docker (Preview)" workload

## Wiki
For more information on how to use Discore, see our [documentation right here on GitHub](https://github.com/BundledSticksInkorperated/Discore/wiki).

## Example Bot: Ping Pong
```csharp
using Discore;
using Discore.WebSocket;
using System;
using System.Threading;

namespace DiscorePingPong
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create authenticator using a bot user token.
            DiscordBotUserToken token = new DiscordBotUserToken("<bot user token goes here>");

            // Create a WebSocket application.
            DiscordWebSocketApplication app = new DiscordWebSocketApplication(token);

            // Create and start a single shard.
            Shard shard = app.ShardManager.CreateSingleShard();
            shard.Start();

            // Subscribe to the message creation event.
            shard.Gateway.OnMessageCreated += Gateway_OnMessageCreated;

            // Wait for the shard to end before closing the program.
            while (shard.IsRunning)
                Thread.Sleep(100);
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
