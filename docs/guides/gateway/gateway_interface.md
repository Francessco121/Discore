# Gateway Interface
Communication with Discord's WebSocket API happens through the [Gateway](https://discord.com/developers/docs/topics/gateway). To interact with the Gateway, an instance of [`Discore.WebSocket.IDiscordGateway`](xref:Discore.WebSocket.IDiscordGateway) is needed. This can be retrieved from any `Shard` via the [`Gateway`](xref:Discore.WebSocket.Shard.Gateway) property.

## Gateway Events
The `IDiscordGateway` interface contains C# events for most events [documented by the Discord API](https://discord.com/developers/docs/topics/gateway-events#receive-events). The [full list of supported events can be viewed here](xref:Discore.WebSocket.IDiscordGateway#events). The names of each C# event follow the pattern "On" + the name of the Gateway event.

For example, to listen for the [Message Create event](https://discord.com/developers/docs/topics/gateway-events#message-create):

```csharp
gateway.OnMessageCreate += Gateway_OnMessageCreate;

...

void Gateway_OnMessageCreate(object? sender, MessageCreateEventArgs e)
{
    // Handle the message
}
```

Note: the `sender` argument for each event is **always** the `IDiscordGateway` instance (despite it being nullable).

## Gateway Methods
The `IDiscordGateway` interface exposes methods for making requests to the Gateway, such as:
- Sending self-user [presence updates](https://discord.com/developers/docs/topics/gateway-events#update-presence).
- [Requesting guild member chunk](https://discord.com/developers/docs/topics/gateway-events#request-guild-members) payloads.

Available methods [can be viewed here](xref:Discore.WebSocket.IDiscordGateway#methods).

For example, the bot's user status can be set by doing:
```csharp
// Makes the bot appear to be playing Tunic.
await gateway.UpdatePresenceAsync(new PresenceOptions()
    .AddActivity(new ActivityOptions("Tunic")));
```

Gateway methods will wait until the underlying connection is fully connected. This means that these methods are perfectly safe to use even during an automatic shard reconnection, but the returned `Task` will take a little longer to complete. However, if the shard is stopped in the middle of a method call, an `OperationCanceledException` will be thrown.