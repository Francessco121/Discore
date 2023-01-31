# Voice Connections
To connect to a voice channel, you first need a Gateway connection. Voice connections are tied to the Gateway connection that they originate from. Additionally, the guild that the application connects to must be served by the Gateway connection (shard) that is used.

> [!WARNING]
> Each guild may only have one active voice connection with your application at a time.

> [!NOTE]
> Voice connections require the [`OnVoiceStateUpdate`](xref:Discore.WebSocket.IDiscordGateway.OnVoiceStateUpdate) Gateway event and as such the shard must be started with the [`GuildVoiceStates`](xref:Discore.WebSocket.GatewayIntent.GuildVoiceStates) Gateway intent. Voice connections will otherwise never connect!

## Create a Voice Connection
To create a voice connection for a guild, simply instantiate a new [`DiscordVoiceConnection`](xref:Discore.Voice.DiscordVoiceConnection):

```csharp
// Get the ID of the guild you wish to create a voice connection to.
Snowflake guildId = ...;

// Create a connection.
var connection = new DiscordVoiceConnection(shard, guildId);
```

## Initiate the Connection
Creating a `DiscordVoiceConnection` does not actually initiate the connection, this must be done manually. This gives applications a chance to subscribe to events such as [`DiscordVoiceConnection.OnConnected`](xref:Discore.Voice.DiscordVoiceConnection.OnConnected).

```csharp
// Get the ID of the voice channel you wish to connect to.
Snowflake voiceChannelId = ...;

try
{
    // Initiate the connection.
    await connection.ConnectAsync(voiceChannelId);

    // Successfully connected!
    // ...
}
catch (OperationCanceledException)
{
    // Failed to connect...
}
```

It's as easy as that! The [`OnConnected`](xref:Discore.Voice.DiscordVoiceConnection.OnConnected) event will be fired when the handshake is finished.

## Connection Invalidation
Unlike the Gateway connection, voice connections do not automatically reconnect when a fatal connection error occurs. Therefore, when a voice connection disconnects (either normally or from an error) it is considered invalid. When this occurs, the [`OnInvalidated`](xref:Discore.Voice.DiscordVoiceConnection.OnInvalidated) event will be fired with a reason for the invalidation. You can check if a connection is invalid beforehand using the [`IsValid`](xref:Discore.Voice.DiscordVoiceConnection.IsValid) property.

When a `DiscordVoiceConnection` is invalid, any attempt to use it will result in a no-op instead of an exception being thrown.

Invalidation occurs during the following scenarios:
- Failed handshake
- Normal disconnection
- Any fatal connection error
- Disposing of the connection

## Clean Up
When you are finished with a `DiscordVoiceConnection` instance, please be sure to dispose it:
```csharp
connection.Dispose();
```

This will ensure the connection is closed and release underlying socket resources.

> [!NOTE]
> When shards are stopped/disposed, their associated `DiscordVoiceConnection`s are not automatically cleaned up. Please make sure you dispose of all active voice connections when attempting to gracefully stop a shard. 

## Switching Channels
Once connected, a `DiscordVoiceConnection` can be moved to another voice channel within the same guild by [updating the voice state](xref:Discore.Voice.DiscordVoiceConnection.UpdateVoiceStateAsync*):
```csharp
await connection.UpdateVoiceStateAsync(newVoiceChannelId);
```

> [!NOTE]
> This method will return **before** the application has fully switched channels. Instead, listen for the `OnConnected` event to know when the switch has completed.

## Changing Self-Muted/Deafened States
In addition to switching voice channels, `UpdateVoiceStateAsync` can also be used to change the muted and deafened states of the bot user:
```csharp
// Will stay in the same voice channel, but become mute and deaf
await connection.UpdateVoiceStateAsync(connection.VoiceChannelId,
    isMute: true,
    isDeaf: true);
```

> [!TIP]
> This can also be done initially as part of the `ConnectAsync` call.

---
Next: [Sending Voice Data](./sending.md)
