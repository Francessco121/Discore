# Getting Started
A short guide for getting up and running with Discore.

## 0. Check .NET version
**Discore currently targets .NET 6.0.**

Discore targets the most recent LTS version of .NET. Newer versions of .NET are compatible. Notably, .NET Core 3.x and .NET Framework are not supported.

## 1. Acquire Discore
[![NuGet](https://img.shields.io/nuget/v/Discore.svg?style=flat-square)](https://www.nuget.org/packages/Discore/)

Discore can be added as a project dependency in a few ways:

#### [.NET CLI](#tab/dotnet-cli)
Run the following in your `.csproj` directory:
```
dotnet add package Discore --version 5.0.1
```
#### [PackageReference](#tab/package-reference)
Add the following to your `.csproj`:
```xml
<PackageReference Include="Discore" Version="5.0.1" />
```
#### [Source](#tab/source)
Download the [the source code](https://github.com/Francessco121/Discore) and add `src/Discore/Discore.csproj` as a local project reference.
```xml
<ProjectReference Include="<path to repo>/src/Discore/Discore.csproj" />
```

---

## 2. Try the "Ping Pong" bot
Here's a quick sample bot to get up and running. Just enter your bot's user token for the `TOKEN` constant and fire away!

> [!NOTE]
> Your bot must have the `MessageContent` intent enabled in the [developer portal](https://discord.com/developers/applications)!

```csharp
using Discore;
using Discore.Http;
using Discore.WebSocket;
using System;
using System.Threading.Tasks;

namespace DiscorePingPong;

class Program
{
    static DiscordHttpClient? http;

    public static async Task Main(string[] args)
    {
        const string TOKEN = "<bot user token goes here>";

        // Create an HTTP client
        http = new DiscordHttpClient(TOKEN);

        // Create a single shard
        using var shard = new Shard(TOKEN, 0, 1);

        // Subscribe to the message creation event
        shard.Gateway.OnMessageCreate += OnMessageCreate;

        // Start the shard
        await shard.StartAsync(GatewayIntent.GuildMessages | GatewayIntent.MessageContent);
        Console.WriteLine("Bot started!");

        // Wait for the shard to stop before closing the program
        await shard.WaitUntilStoppedAsync();
    }

    static async void OnMessageCreate(object? sender, MessageCreateEventArgs e)
    {
        DiscordMessage message = e.Message;

        if (message.Author.Id == e.Shard.UserId)
            // Ignore messages created by our bot
            return;

        if (message.Content == "!ping")
        {
            try
            {
                // Reply to the user who posted "!ping"
                await http!.CreateMessage(message.ChannelId, $"<@!{message.Author.Id}> Pong!");
            }
            catch (DiscordHttpApiException) { /* Message failed to send... :( */ }
        }
    }
}
```

## 3. Check out the samples (optional)
For additional full bot examples, check out [the samples directory](https://github.com/Francessco121/Discore/tree/v5/samples) in the repository.

## 4. Start building your own bot
All that's left now is to build your bot!

Depending on your application's needs, you will need access to the HTTP API, the real-time WebSocket Gateway, or more likely both.

See the following major sections to learn more:
- [HTTP Clients](./http/http_client.md)
- [Shards](./gateway/shards.md)
