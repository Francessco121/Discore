using Discore;
using Discore.Http;
using Discore.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Discore.Samples.PingPong;

class Program : IDisposable
{
    readonly DiscordHttpClient http;
    readonly Shard shard;

    public static async Task Main(string[] args)
    {
        // Load the bot token
        string token = (await File.ReadAllTextAsync("TOKEN.txt")).Trim();

        // Run bot
        using var program = new Program(token);
        await program.Run();
    }

    public Program(string token)
    {
        // Create an HTTP client
        http = new DiscordHttpClient(token);

        // Create a single shard
        shard = new Shard(token, 0, 1);

        // Subscribe to the message creation event
        shard.Gateway.OnMessageCreate += Gateway_OnMessageCreate;
    }

    public void Dispose()
    {
        // Clean up
        http.Dispose();
        shard.Dispose();
    }

    public async Task Run()
    {
        // Start the shard
        await shard.StartAsync(GatewayIntent.GuildMessages | GatewayIntent.MessageContent);

        Console.WriteLine("Bot started!");

        // Let Ctrl-C stop the shard
        Console.WriteLine("Press Ctrl-C to stop.");

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;

            if (shard.IsRunning)
            {
                Console.WriteLine("Stopping...");
                shard.StopAsync().Wait();
            }
        };

        // Wait for the shard to end before closing the program
        await shard.WaitUntilStoppedAsync();
    }

    async void Gateway_OnMessageCreate(object? sender, MessageCreateEventArgs e)
    {
        DiscordMessage message = e.Message;

        if (message.Author.Id == shard.UserId)
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
