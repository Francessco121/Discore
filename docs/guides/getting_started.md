# Getting Started
A short guide for getting up and running with Discore.

## 0. Check .NET Version
**Discore currently targets .NET 6.0.**

Discore targets the most recent LTS version of .NET. Newer versions of .NET are compatible. Notably, .NET Core 3.x and .NET Framework are not supported.

## 1. Acquire Discore
[![NuGet](https://img.shields.io/nuget/v/Discore.svg?style=flat-square)](https://www.nuget.org/packages/Discore/)

Discore can be added as a project dependency in a few ways:

#### [.NET CLI](#tab/dotnet-cli)
Run the following in your `.csproj` directory:
```
dotnet add package Discore --version 5.0.0
```
#### [PackageReference](#tab/package-reference)
Add the following to your `.csproj`:
```xml
<PackageReference Include="Discore" Version="5.0.0" />
```
#### [Source](#tab/source)
Download the [the source code](https://github.com/Francessco121/Discore) and add `src/Discore/Discore.csproj` as a local project reference.
```xml
<ProjectReference Include="<path to repo>/src/Discore/Discore.csproj" />
```

---

## 2. (Optional) Try the Example Bot
If you wish to test your Discore installation, try this example bot. Just enter your bot's user token for the `TOKEN` constant and fire away!

```csharp
using Discore;
using Discore.Http;
using Discore.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscorePingPong;

public class Program
{
    DiscordHttpClient http;

    public static async Task Main()
    {
        var program = new Program();
        await program.Run();
    }

    public async Task Run()
    {
        const string TOKEN = "<bot user token goes here>";

        // Create an HTTP client.
        http = new DiscordHttpClient(TOKEN);

        // Create a single shard.
        using (var shard = new Shard(TOKEN, 0, 1))
        {
            // Subscribe to the message creation event.
            shard.Gateway.OnMessageCreate += Gateway_OnMessageCreate;

            // Start the shard.
            await shard.StartAsync();
            Console.WriteLine("Bot started!");

            // Wait for the shard to end before closing the program.
            await shard.WaitUntilStoppedAsync();
        }
    }

    async void Gateway_OnMessageCreate(object? sender, MessageCreateEventArgs e)
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
```

## 3. (Optional) Check out other examples
Discore has a repository dedicated to sample bots, which [can be found here](https://github.com/Francessco121/Discore.Samples).

## 4. Start building your own bot
All that's left now is to build your bot!

See the following major sections to learn more:
- [HTTP Clients](./http/http_client.md)
- [Shards](./gateway/shards.md)
