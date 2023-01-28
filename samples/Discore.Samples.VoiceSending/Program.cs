using Discore;
using Discore.Caching;
using Discore.Http;
using Discore.Voice;
using Discore.WebSocket;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Discore.Samples.VoiceSending;

class Program : IDisposable
{
    readonly ConcurrentDictionary<Snowflake, VoiceSession> voiceSessions = new();
    readonly DiscordHttpClient http;
    readonly Shard shard;
    readonly DiscordMemoryCache cache;

    static async Task Main(string[] args)
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

        // Listen for shard stops so we can clean up our voice sessions
        shard.OnDisconnected += Shard_OnDisconnected;

        // For convenience, we'll use an in-memory cache to capture entities
        // such as voice states, which we'll need later
        cache = new DiscordMemoryCache(shard);

        // Subscribe to the message creation event.
        shard.Gateway.OnMessageCreate += Gateway_OnMessageCreate;
    }

    public void Dispose()
    {
        // Clean up
        http.Dispose();
        shard.Dispose();
        cache.Dispose();
    }

    public async Task Run()
    {
        // Start the shard.
        //
        // We'll need:
        // - Guilds and their channels
        // - Message events
        // - Member voice states
        // - Actual message text content
        await shard.StartAsync(
            GatewayIntent.Guilds |
            GatewayIntent.GuildMessages |
            GatewayIntent.GuildVoiceStates |
            GatewayIntent.MessageContent);

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

    /// <summary>
    /// Responds to the invoker of the specified message.
    /// </summary>
    async Task Respond(DiscordMessage to, string withMessage)
    {
        await http!.CreateMessage(to.ChannelId, $"<@!{to.Author.Id}> {withMessage}");
    }

    async void Gateway_OnMessageCreate(object? sender, MessageCreateEventArgs e)
    {
        try
        {
            DiscordMessage message = e.Message;

            if (message.Author.Id == shard.UserId)
                // Ignore all messages sent by our bot
                return;
            if (message.GuildId == null)
                // Ignore DMs
                return;

            // Check if the message is a command
            if (message.Content == "!join")
            {
                await HandleJoinCommand(message, message.GuildId.Value);
            }
            else if (message.Content == "!leave")
            {
                await HandleLeaveCommand(message, message.GuildId.Value);
            }
            else if (message.Content.StartsWith("!play"))
            {
                await HandlePlayCommand(message, message.GuildId.Value);
            }
            else if (message.Content == "!stop")
            {
                await HandleStopCommand(message, message.GuildId.Value);
            }
        }
        // It's very important to catch all exceptions in this handler since the method is async void!
        // The process will quit if an exception is not handled in this method.
        catch (Exception ex)
        {
            Console.WriteLine($"An unhandled exception occured while processing a message: {ex}");
        }
    }

    async Task HandleJoinCommand(DiscordMessage message, Snowflake guildId)
    {
        // First, figure out what voice channel the invoking user is in
        //
        // We'll always join the voice channel that the user is in, regardless
        // of what text channel we received the command from.
        DiscordVoiceState? voiceState = cache.GetVoiceState(guildId, message.Author.Id);

        if (voiceState == null || voiceState.ChannelId == null)
        {
            // Either the voice state is not cached (unlikely but possible) or the user
            // is not currently in a voice channel
            await Respond(message, "You are not in a voice channel!");
            return;
        }

        Snowflake voiceChannelId = voiceState.ChannelId.Value;

        // If we are already connected in this guild, move to the new voice channel
        VoiceSession? session;
        if (voiceSessions.TryGetValue(guildId, out session) && session.IsValid)
        {
            await session.ConnectOrMove(voiceChannelId);
            return;
        }

        // Check if the bot has permission to join
        if (!CanBotJoin(guildId, voiceChannelId))
        {
            await Respond(message, ":frowning: I can't join that voice channel.");
            return;
        }

        // Create a new voice connection
        var connection = new DiscordVoiceConnection(shard, guildId);

        // Subscribe to the invalidation event so we can clean up our voice session
        connection.OnInvalidated += Connection_OnInvalidated;

        // Create and add our new voice session
        session = new VoiceSession(connection);

        if (!voiceSessions.TryAdd(guildId, session))
        {
            // If this fails, then we already have a session for this guild
            // This should only happen if more than one join command is issued concurrently
            session.Dispose();
            return;
        }

        try
        {
            // Connect to the voice channel
            await session.ConnectOrMove(voiceChannelId);

            // Success!
            await Respond(message, "Hello!");
        }
        catch (OperationCanceledException)
        {
            // Connection failed
            await Respond(message, ":frowning: Sorry, I can't seem to connect at the moment.");
        }
    }

    async Task HandleLeaveCommand(DiscordMessage message, Snowflake guildId)
    {
        // Remove and get the existing session for the guild
        if (voiceSessions.TryRemove(guildId, out VoiceSession? session))
        {
            // Disconnect the session
            await session.Disconnect();

            // Success!
            await Respond(message, "Bye!");
        }
        else
        {
            await Respond(message, "I'm not in a voice channel!");
        }
    }

    async Task HandlePlayCommand(DiscordMessage message, Snowflake guildId)
    {
        // Get the current session for the guild
        if (voiceSessions.TryGetValue(guildId, out VoiceSession? session))
        {
            // Get the uri from the message
            if (message.Content.Length >= 6)
            {
                string uri = message.Content.Substring(6).Trim();

                if (!string.IsNullOrWhiteSpace(uri))
                {
                    // Notify the user we're starting
                    await Respond(message, $"Playing {uri}...");

                    // Play the audio uri
                    await session.Play(uri);
                }
                else
                    await Respond(message, "Usage: !play <uri>");
            }
            else
                await Respond(message, "Usage: !play <uri>");
        }
        else
        {
            await Respond(message, "I'm not in a voice channel!");
        }
    }

    async Task HandleStopCommand(DiscordMessage message, Snowflake guildId)
    {
        // Get the current session for the guild
        if (voiceSessions.TryGetValue(guildId, out VoiceSession? session))
        {
            // Stop the current play task
            if (session.Stop())
            {
                // Success!
                await Respond(message, "Stopped playing audio.");
            }
        }
        else
        {
            await Respond(message, "I'm not in a voice channel!");
        }
    }

    void Connection_OnInvalidated(object? sender, VoiceConnectionInvalidatedEventArgs e)
    {
        e.Connection.OnInvalidated -= Connection_OnInvalidated;

        if (e.Reason != VoiceConnectionInvalidationReason.Normal &&
            e.Reason != VoiceConnectionInvalidationReason.TimedOut)
        {
            // Something serious went wrong...
            Console.WriteLine($"[{e.Reason}] {e.ErrorMessage}");
        }

        // Remove our respective voice session for this connection
        if (voiceSessions.TryRemove(e.Connection.GuildId, out VoiceSession? session))
            session.Dispose();
    }

    void Shard_OnDisconnected(object? sender, ShardEventArgs e)
    {
        // Ensure each voice session is stopped and cleaned up
        foreach (VoiceSession session in voiceSessions.Values)
            session.Dispose();

        voiceSessions.Clear();
    }

    bool CanBotJoin(Snowflake guildId, Snowflake voiceChannelId)
    {
        // To determine if the bot can join, we'll need a few entities
        DiscordGuildMember? member = cache.GetGuildMember(guildId, shard.UserId!.Value); // Bot's member entity
        DiscordGuild? guild = cache.GetGuild(guildId); // The guild itself
        DiscordGuildVoiceChannel? voiceChannel = cache.GetGuildVoiceChannel(voiceChannelId); // The voice channel
        int? usersInChannel = cache.GetUsersInVoiceChannel(voiceChannelId)?.Count; // Number of users already in the channel

        // If we're missing any of these in the cache, we can't do the permission check
        // For now, we can return true and attempt the connection anyway (Discord will
        // let the connection time out if we can't join)
        if (member == null || guild == null || voiceChannel == null)
            return true;

        // Check permissions!
        return DiscordPermissionHelper.CanJoinVoiceChannel(member, guild, voiceChannel, usersInChannel);
    }
}
