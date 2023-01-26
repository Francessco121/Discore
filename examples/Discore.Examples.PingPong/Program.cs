using Discore;
using Discore.Http;
using Discore.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Discore.Samples.PingPong;

class Program
{
    DiscordHttpClient? http;

    public static async Task Main(string[] args)
    {
        var program = new Program();
        await program.Run();
    }

    public async Task Run()
    {
        // Get bot token.
        string token = (await File.ReadAllTextAsync("TOKEN.txt")).Trim();

        // Create an HTTP client.
        http = new DiscordHttpClient(token);

        // Create a single shard.
        using Shard shard = new Shard(token, 0, 1);

        // Subscribe to the message creation event.
        shard.Gateway.OnMessageCreate += Gateway_OnMessageCreate;

        // Start the shard.
        await shard.StartAsync(GatewayIntent.GuildMessages | GatewayIntent.MessageContent);
        Console.WriteLine("Bot started!");

        // Wait for the shard to end before closing the program.
        await shard.WaitUntilStoppedAsync();
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
                await http!.CreateMessage(message.ChannelId, $"<@!{message.Author.Id}> Pong!");
            }
            catch (DiscordHttpApiException) { /* Message failed to send... :( */ }
        }
    }
}
