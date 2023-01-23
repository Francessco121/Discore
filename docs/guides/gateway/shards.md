# Shards
In order to work with Discord's WebSocket API, an instance of [`Discore.WebSocket.Shard`](xref:Discore.WebSocket.Shard) is required.

A shard represents a single [Gateway](https://discord.com/developers/docs/topics/gateway) connection. For larger bots (those in many guilds), Discord requires multiple shards to be created. When multiple shards for a single bot exist, each shard is responsible for a different set of guilds. Smaller bots that don't require multiple shards *technically* aren't using sharding, however Discore still requires an instance of `Shard` as an entrypoint to the WebSocket API.

For more information on how sharding works with the Discord API, [see Discord's documentation](https://discord.com/developers/docs/topics/gateway#sharding) on the topic.

## Creating Shards

Creating a shard is as easy as instantiating a `Shard` class with the bot's user token, the shard ID, and the total number of shards. For smaller bots not using sharding, the shard ID should be zero and the total number of shards set to one.

```csharp
// All that's needed for a single-shard application
var shard = new Shard(TOKEN, shardId: 0, totalShards: 1);
```

### Determining How Many Shards an Application Requires
The Discord HTTP API can return the minimum number of shards any given Discord application requires to use the WebSocket API. This is available in Discore through [`DiscordHttpClient.GetBotRequiredShards`](xref:Discore.Http.DiscordHttpClient.GetBotRequiredShards).

This endpoint is very useful for public Discord bots, as sharding will be required once a bot is serving a certain number of guilds.

> [!TIP]
> Shards do not need to be created within the same .NET process! You can scale your bot out horizontally across multiple servers for example and have each server use a certain shard ID (or set of IDs). This does require additional coordination however, as you will probably need some kind of "master service" that spins up servers depending on the shard count.

Example usage:

```csharp
// Get number of required shards
int totalShards = await httpClient.GetBotRequiredShards();

// Create each shard
Shard[] shards = new Shard[totalShards];
for (int i = 0; i < totalShards; i++)
    shards[i] = new Shard(TOKEN, i, totalShards);
```

## Starting a Shard
Before the WebSocket API can be used, the shard needs to be "started" through the [`StartAsync`](xref:Discore.WebSocket.Shard.StartAsync*) method. `StartAsync` will return a `Task` that completes when the underlying [Gateway](https://discord.com/developers/docs/topics/gateway) connection has fully completed its first handshake. If this handshake fails, a [`ShardStartException`](xref:Discore.WebSocket.ShardStartException) will be thrown.

When starting a shard, you must notify Discord of which [intents](https://discord.com/developers/docs/topics/gateway#gateway-intents) you wish to subscribe to. Intents specify which [Gateway events](https://discord.com/developers/docs/topics/gateway-events#receive-events) your shard will receive. This is passed to `StartAsync` as a bitwise OR of the [`GatewayIntent`](xref:Discore.WebSocket.GatewayIntent) enum. You can [view a list of intents and the events they enable here](https://discord.com/developers/docs/topics/gateway#list-of-intents).

> [!CAUTION]
> Developers should take care when making use of [privileged intents](https://discord.com/developers/docs/topics/gateway#privileged-intents). Please read [Discord's documentation on the topic](https://discord.com/developers/docs/topics/gateway#privileged-intents).
>
> Privileged intents must be manually enabled in the [Developer Portal](https://discord.com/developers/applications) and for bots that are large enough to require verification, they must also be approved for each intent.

> [!NOTE]
> Creating voice connections requires the gateway intent `GuildVoiceStates`!

Example:
```csharp
// Determine intents
//
// Note: This is just an example, only enable the intents that you need.
var intents = 
    GatewayIntent.Guilds |
    GatewayIntent.GuildBans |
    GatewayIntent.GuildVoiceStates;

// Start the shard
await shard.StartAsync(intents);
```

If you don't need any intents, then you can just pass `GatewayIntent.None`.

Optionally, you may instead pass a [`ShardStartConfig`](xref:Discore.WebSocket.ShardStartConfig) to `StartAsync`. This class allows additional shard-specific configuration of the underlying connection.

## Automatic Shard Reconnection
Once a shard has been started, it will continue to reconnect/resume as needed ([see the Discord documentation for more information](https://discord.com/developers/docs/topics/gateway#resuming)) unless a fatal error is encountered. Everytime the `Shard` automatically reconnects, the `OnReconnected` event will be fired specifying whether the connection resumed or created a new session.

The [`OnReconnected`](xref:Discore.WebSocket.Shard.OnReconnected) event is especially useful for maintaining a bot's user status. Everytime the underlying connection is forced to create a new session, this status is cleared.

## Shard Failures
If the `Shard` runs into an error it cannot safely recover from, the shard will enter the stopped state and the [`OnFailure`](xref:Discore.WebSocket.Shard.OnFailure) event will be fired with the reason for the failure.

## Re-sharding
As your bot grows, Discord may require additional shards while your application is running. In this case, you may encounter the [`ShardFailureReason`](xref:Discore.WebSocket.ShardFailureReason) of `ShardingRequired` or `ShardInvalid` during the `StartAsync` call or through the `OnFailure` event. In these cases, you will need to [determine the new shard count required for your bot](#determining-how-many-shards-an-application-requires) and most likely need to recreate each shard. 

---
Next: [Gateway Interface](./gateway_interface.md)
