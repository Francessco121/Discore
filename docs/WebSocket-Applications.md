[[← back]](./README.md)

# WebSocket Applications

WebSocket applications are meant for realtime interactions with the Discore API, such as a bot user. Using Discore for WebSocket use will give your application access to Discord's Gateway API.

To use Discore for a WebSocket application is very simple:
```csharp
// Create the WebSocket application wrapper
using (DiscordWebSocketApplication app = new DiscordWebSocketApplication(authenticator))
{
    ...
}
```

For creating the `authenticator`, see the [authentication documentation](./Authentication.md).

## Connecting to the Gateway API
`DiscordWebSocketApplication` provides a `ShardManager` to create connections with the Gateway API. This can be accessed through `DiscordWebSocketApplication.ShardManager`.

To make a single connection to the Gateway API:
```csharp
// Create a single shard for our application.
Shard shard = app.ShardManager.CreateSingleShard(); // assuming app is a DiscordWebSocketApplication
// Start the shard, which will initiate the Gateway connection.
await shard.StartAsync(CancellationToken.None);
```

For more information on using shards and the Gateway API, [see the main documentation](./Sharding.md).

## HTTP API Usage
Even though we have not created a `DiscordHttpApplication`, the HTTP API can still be accessed through `DiscordWebSocketApplication.HttpApi`:
```csharp
DiscordHttpApi httpApi = app.HttpApi; // assuming app is a DiscordWebSocketApplication
```

See the main HTTP API documentation [here](./HTTP-Applications.md#http-api-interface).