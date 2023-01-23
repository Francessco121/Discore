# Updating Messages
Since the Discord API only sends partial versions of messages on a message update event, Discore provides a way to take an older version of a message and combine it with the newer partial.

Discore does not cache messages locally, so in order to do this effectively the application will need to store them manually. For the sake of example, lets say messages for a particular channel are saved in a dictionary:

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

Now when we receive a partial message from the message update event, we can take the older version from the dictionary and combine them to get the full updated message:

```csharp
// Subscribe to message update event.
shard.Gateway.OnMessageUpdated += Gateway_OnMessageUpdated;

...

void Gateway_OnMessageUpdated(object sender, MessageUpdateEventArgs e)
{
    // Get the old and new partial messages.
    DiscordMessage oldMessage = messages[e.PartialMessage.Id];
    DiscordMessage newPartialMessage = e.PartialMessage;

    // Create a new message object by combining the old message with the newer partial.
    // Note: oldMessage and newPartialMessage are not changed by this.
    DiscordMessage newFullMessage = DiscordMessage.Update(oldMessage, newPartialMessage);

    // Update the message cache.
    messages[newFullMessage.Id] = newFullMessage;
}
```
