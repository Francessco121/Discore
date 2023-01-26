# Partial Messages
Since the Gateway only sends partial versions of messages on a [`MessageUpdate`](xref:Discore.WebSocket.IDiscordGateway.OnMessageUpdate) event, Discore provides a way to take an older version of a message and combine it with the newer partial, yielding a new full message.

This can be achieved by using the [`DiscordMessage.Update`](xref:Discore.DiscordMessage.Update*) static function.

Discore does not cache messages, so applications will need to store messages themselves. For the sake of example, let's say received messages are saved in a dictionary:

```csharp
ConcurrentDictionary<Snowflake, DiscordMessage> messages = ...;

...

// Subscribe to message creation event.
shard.Gateway.OnMessageCreate += Gateway_OnMessageCreate;

...

void Gateway_OnMessageCreate(object? sender, MessageCreateEventArgs e)
{
    // Add each new message to the dictionary.
    messages.TryAdd(e.Message.Id, e.Message);
}
```

Now, when we receive a partial message from the `MessageUpdate` event, we can take the older version from the dictionary and combine it with the new partial to get the full updated message:

```csharp
// Subscribe to message update event.
shard.Gateway.OnMessageUpdate += Gateway_OnMessageUpdate;

...

void Gateway_OnMessageUpdate(object? sender, MessageUpdateEventArgs e)
{
    // Get the old and new partial messages.
    DiscordMessage oldMessage = messages[e.PartialMessage.Id];
    DiscordPartialMessage newPartialMessage = e.PartialMessage;

    // Create a new message object by combining the old message with the newer partial.
    // Note: oldMessage and newPartialMessage are not changed by this.
    DiscordMessage newFullMessage = DiscordMessage.Update(oldMessage, newPartialMessage);

    // Update the message cache.
    messages[newFullMessage.Id] = newFullMessage;
}
```
