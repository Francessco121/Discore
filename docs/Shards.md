[[← back]](./README.md)

# Shards

In order to work with Discord's WebSocket API through Discore, an instance of `Discore.WebSocket.Shard` is required.

For information on how sharding works with the Discord API, [see Discord's documentation](https://discordapp.com/developers/docs/topics/gateway#sharding).

## Creating Shards

Creating a shard is as easy as instantiating `Discore.WebSocket.Shard` with the bot's user token, the shard ID, and the total number of shards:

```csharp
// All that's needed for a single-shard application
Shard shard = new Shard(TOKEN, shardId: 0, totalShards: 1);
```

### Determining How Many Shards an Application Requires
The Discord HTTP API can return the minimum number of shards any given Discord application requires to use the WebSocket API. This is available in Discore as the `GetBotRequiredShards` method on `DiscordHttpClient`.

This endpoint is very useful for public Discord bot's, as sharding will be required once a bot is serving a certain number of guilds.

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
Before the WebSocket API can be used, the shard needs to be "started" through the `StartAsync` method. `StartAsync` will return a `Task` that completes when the underlying [Gateway](https://discordapp.com/developers/docs/topics/gateway) connection has fully completed its first handshake. If this handshake fails, a `ShardStartException` will be thrown.

An optional instance of `ShardStartConfig` can be passed to `StartAsync`. This class allows shard-specific configuration of the underlying connection.

Example:
```csharp
// Note: config is optional
ShardStartConfig config = new ShardStartConfig();
config.GatewayLargeThreshold = 50; // Use minimum threshold

// Start the shard
await shard.StartAsync(config);
```

## Automatic Shard Reconnection
Once a shard has been started, it will continue to reconnect/resume as needed ([see the Discord documentation for more information](https://discordapp.com/developers/docs/topics/gateway#resuming)) unless a fatal error is encountered. Everytime the `Shard` automatically reconnects, the `OnReconnected` event will be fired specifying whether the connection resumed or created a new session.

The `OnReconnected` event is especially useful for maintaining a bot's user status. Everytime the underlying connection is forced to create a new session, this status is cleared.

## Shard Failures
If the `Shard` runs into an error it cannot safely reconnect from, the shard will enter the stopped state and the `OnFailure` event will be fired with the reason for the failure.

## Continuation
See [the Gateway interface documentation](./Gateway-Interface.md) for more information on interacting with Discord's WebSocket API.