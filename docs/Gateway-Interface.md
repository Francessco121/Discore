[[← back]](./README.md)

# Gateway Interface

Most of the interaction with Discord's WebSocket API is through the [Gateway](https://discord.com/developers/docs/topics/gateway). To interact with the Gateway through Discore, an instance of `IDiscordGateway` is needed. This can be retrieved from any `Shard` via the `Gateway` property.

## Gateway Events
The `IDiscordGateway` interface contains events for most of the events [documented by the Discord API](https://discord.com/developers/docs/topics/gateway#commands-and-events-gateway-events). The names of each event follow the pattern "On" + the past tense version of the event's name.

For example, to listen for the [message create event](https://discord.com/developers/docs/topics/gateway#message-create):

```csharp
gateway.OnMessageCreated += Gateway_OnMessageCreated;

...

void Gateway_OnMessageCreated(object sender, MessageEventArgs e)
{
    // Handle the message
}
```

Note: the `sender` argument for each event is always the `IDiscordGateway` instance.

## Gateway Methods
Currently, the `IDiscordGateway` interface exposes methods for sending [status update](https://discord.com/developers/docs/topics/gateway#update-status) and [request guild members](https://discord.com/developers/docs/topics/gateway#request-guild-members) payloads.

For example, the bot's user status can be set by:
```csharp
// Makes the bot appear to be playing Overwatch.
await gateway.UpdateStatusAsync("Overwatch");
```

**Note:** Each of these methods will wait until the underlying Gateway connection is fully connected. This means that these methods are perfectly safe to use even during an automatic shard reconnection, but the returned `Task` will take a little longer to complete.
