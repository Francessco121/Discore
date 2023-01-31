# 4.x to 5.x Migration Guide
Welcome to Discore v5!

Discore v5 was created from the following goals:
- Support the latest Discord API/protocol versions
- Make Discore more flexible
- Make use of newer .NET features

Because of these decently large goals, Discore v5 introduces a fair number of changes. Most of these changes are very tiny such as renames, deprecated feature removals, and nullability changes. However, there are also a few larger architectural changes to the library. This document will focus on giving guidance for just the large changes. For the full list of changes, please [see the CHANGELOG](../updates.md#v500).

## Discord entity methods are removed
Discord entities (such as `DiscordMessage`) no longer have methods such as `DiscordMessage.Edit`. Internally, these methods were made possible by letting each entity contain a reference to a `DiscordHttpClient`. This however had hidden consequences. Despite all entities being immutable snapshots of entity state, these methods both: made it look as if the entity could be modified and would stop working if the underlying `DiscordHttpClient` was disposed.

These methods are removed in favor of just using the `DiscordHttpClient` methods instead. To assist with this, many HTTP client methods have new overloads that take entity classes.

Example migration:
```diff
DiscordMessage message = ...;
-await message.Edit("new content");
+await httpClient.EditMessage(message, "new content");
```

This change also paves the way for a potential future update where entity classes can be freely serialized by applications.

## Shard caching is now optional
Shards no longer automatically cache various entities received through the Gateway. This change was made so that applications can just cache what they need and save on memory. Due to this change, the `Shard.Cache` property was removed.

For convenience, replacing `DiscordShardCache` is the new `Discore.Caching.DiscordMemoryCache`. This new class is nearly identical to the old cache and can be used to very easily migrate existing code. To use it, simply instantiate the class with your shard:
```csharp
var shard = new Shard(...);
var cache = new DiscordMemoryCache(shard);
```

Don't forget to also dispose of the cache when you're finished with it or when you're disposing the shard!

## Voice connections are now decoupled from `Shard` instances
Voice connections no longer need to exist within the same process as a `Shard` instance. With the new voice bridge feature, applications can run voice connections in different processes or even on different servers. As a consequence of this, `DiscordVoiceConnection`s are now instantiated manually and `Shard.Voice`/`ShardVoiceManager` have been removed.

The removed `ShardVoiceManager` was actually super simple and applications looking for a quick migration can recreate it like so:
```csharp
class MyShardVoiceManager : IDisposable
{
    readonly ConcurrentDictionary<Snowflake, DiscordVoiceConnection> connections = new();
    readonly Shard shard;

    public MyShardVoiceManager(Shard shard)
    {
        this.shard = shard;
    }

    public void Dispose()
    {
        // Ensure each connection is disposed
        foreach (DiscordVoiceConnection connection in connections.Values)
            connection.Dispose();

        connections.Clear();
    }

    public DiscordVoiceConnection GetOrCreate(Snowflake guildId)
    {
        DiscordVoiceConnection? connection;
        if (!connections.TryGetValue(guildId, out connection) ||
            !connection.IsValid)
        {
            // Create new connection
            connection = new DiscordVoiceConnection(shard, guildId);
            connection.OnInvalidated += Connection_OnInvalidated;

            if (!connections.TryAdd(guildId, connection))
            {
                // Concurrency error, use existing connection
                connection.Dispose();
                return connections[guildId];
            }
            else
            {
                return connection;
            }
        }
        else
        {
            // Reuse existing
            return connection;
        }
    }

    void Connection_OnInvalidated(object? sender, VoiceConnectionInvalidatedEventArgs e)
    {
        e.Connection.OnInvalidated -= Connection_OnInvalidated;

        // Connection died, remove it
        if (connections.TryRemove(e.Connection.GuildId, out DiscordVoiceConnection? connection))
        {
            connection.Dispose();
        }
    }
}
```

## Gateway intents
With the move to Discord Gateway v8+, applications are now required to specify which Gateway events they're interested in. When starting a `Shard` instance, you will now need to pass a bitwise OR combination of `GatewayIntent`s:
```csharp
// Note: This is just an example, only enable the intents that you need.
var intents = 
    GatewayIntent.Guilds |
    GatewayIntent.GuildBans |
    GatewayIntent.GuildVoiceStates;

await shard.StartAsync(intents);
```

Discord's documentation contains a [list of intents](https://discord.com/developers/docs/topics/gateway#list-of-intents) and which events they enable.

Gateway intents come in two types: standard and privileged. Privileged intents (such as `MESSAGE_CONTENT`) must be enabled in the [Developer Portal](https://discord.com/developers/applications). Please read [Discord's documentation on privileged intents](https://discord.com/developers/docs/topics/gateway#privileged-intents) carefully. Applications that are large enough to require verification must be approved for these intents before they can be used!

## Message attachments
In previous versions of Discore, message creation/editing only supported uploading a single attachment. With v5, you can now upload as many as Discord allows. Due to this change, the `DiscordHttpClient` `CreateMessage`/`EditMessage` overloads that took attachments are removed and replaced by additional options in `CreateMessageOptions`/`EditMessageOptions`.

Example of uploading attachments in Discore v5:
```csharp
// SetContent has a bunch of overloads, byte[] is just an example here.
// Attachments can also be created from streams, strings, or HttpContent.
byte[] file = ...;

await httpClient.CreateMessage(channelId, new CreateMessageOptions()
    // '0' is an ID unique to the create message payload. The ID is
    // temporary and will be replaced by a real Snowflake after the upload.
    .AddAttachment(new AttachmentOptions(0)
        // Attachments MUST have a filename and content.
        .SetFileName("filename.png")
        .SetContent(file))
```

## .NET/C# changes

### .NET 6
With v5, Discore now targets .NET 6.0. Going forward, Discore will target the most recent LTS version of .NET (so this won't change until the next major version of Discore after .NET 8 releases). With this huge jump from .NET Standard 1.5, Discore no longer supports two key .NET implementations: .NET Core 3.x and .NET Framework. If your application is not currently targeting .NET 6 or higher, you will need to make the switch.

### C# 8 Nullable Reference Types
With C# 8, support for [nullable reference types](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-reference-types) was introduced. In Discore v5, the entire library now annotates every nullable reference type and has changed the nullability of a few properties/methods. For the most part, this won't break applications since incorrect usage of nullable types is by default a warning.

However, there is a notable list of properties/methods that changed:
- `DiscordHttpClient.BeginGuildPrune` now returns null if `computePruneCount` is true.
- `DiscordHttpRateLimitException.Limit` is now nullable.
- `DiscordHttpRateLimitException.Reset` is now nullable.
- `DiscordHttpRateLimitException.ResetHighPrecision` is now nullable.
- `DiscordInvite.Channel` is now nullable.

In all other cases, properties/arguments/return values, etc. that are now annotated as nullable were already nullable in the previous version of Discore.
