[[← back]](./README.md)

# Sharding

In order to work with any WebSocket connection in Discore, a shard is required. If you are working on a public Discord bot, eventually you will need to create multiple shards.

For information on how sharding works with the Discord API, [see Discord's documentation](https://discordapp.com/developers/docs/topics/gateway#sharding).

## Sharding Methods

### Single Shard Applications
If you know your application only requires a single shard or you are testing things, Discore can create and manage a single shard:

```csharp
Shard shard = app.ShardManager.CreateSingleShard();
await shard.StartAsync();
```

### Automatic Minimum Shards
If you are not sure how many shards your application will require or want this to be handled automatically, Discore can query the Discord API and create the minimum number of required shards for your application:

```csharp
await app.ShardManager.CreateMinimumRequiredShardsAsync();
await Task.WhenAll(app.ShardManager.StartShardsAsync());
```

### A Set Of Shards
The Discore `ShardManager` can be used to manage any number of specific shards, rather than all of them. This can be very useful for example if your application creates a new process for each shard.

```csharp
// For this example we will use a bot that requires 6 shards.
int totalShards = 6;

// However, this process will only handle the first 3.
int[] shardIds = new int[] { 0, 1, 2 };

// Create and start the 3 shards.
app.ShardManager.CreateShards(shardIds, totalShards);
await Task.WhenAll(app.ShardManager.StartShardsAsync());
```

If your application needs to automatically get the number of required shards for your application, the HTTP implementation provides a method for this:

```csharp
int requiredShards = await app.HttpApi.Gateway.GetBotRequiredShards();
```

## Gateway Interaction
Each shard instance manages a separate connection to the Discord Gateway API. This is available through `shard.Gateway`. This interface provides events for Discord gateway events, as well as methods for interacting directly with it such as updating a bot user's status.

```csharp
shard.Gateway.OnMessageCreated += Gateway_OnMessageCreated;

...

private static void Gateway_OnMessageCreated(object sender, MessageEventArgs e)
{
    // Someone sent a message somewhere...
}
```

## Voice Connections
Each shard contains a `ShardVoiceManager` available through `Shard.Voice`. Please [see the main voice documentation here](./Voice-Prerequisites.md) for more information.

## Handling Shard Errors
For any non-fatal error a shard's gateway connection encounters, the shard will automatically seamlessly reconnect. However, in the event a shard does encounter a fatal error, the `Shard.OnFailure` event will be fired.

An example of a shard failure is when the gateway fails to authenticate:
```csharp
shard.OnFailure += Shard_OnFailure;

...

private static void Shard_OnFailure(object sender, ShardFailureEventArgs e)
{
    if (e.Reason == ShardFailureReason.AuthenticationFailed)
    {
        // At this point, the entire DiscordWebSocketApplication will most likely need
        // the correct authentication and then be restarted completely... :(
    }
}
```

## Automatic Shard Reconnections
In the event a shard does automatically reconnect internally, the `Shard.OnReconnected` event will be fired. This is important to implement, because some state on your application may be lost, for example the user status.

```csharp
shard.OnReconnected += Shard_OnReconnected;

...

private static async void Shard_OnReconnected(object sender, ShardEventArgs e)
{
    await e.Shard.Gateway.UpdateStatusAsync("I'm a robot!");
}
```