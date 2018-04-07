[[← back]](./README.md)

# Connecting to a Voice Channel

**Note:** It's worth pointing out here that the voice API is only available for WebSocket applications.

Each shard contains a `ShardVoiceManager` available through `Shard.Voice`. The `ShardVoiceManager` keeps track of all open voice connections to guilds that are managed by that shard.

## Create a Voice Connection
```csharp
// Get the voice channel you wish to connect to.
DiscordGuildVoiceChannel voiceChannel = ...;
// Create the connection.
DiscordVoiceConnection connection = shard.Voice.CreateConnection(voiceChannel);
```

**Note:** If a connection to the given voice channel already exists, `CreateConnection` returns the existing connection instead of a new one being created.

## Initiate the Connection
Creating a voice connection does not actually initiate the connection, this must be done manually. This is done manually to allow the application to safely subscribe to events like `DiscordVoiceConnection.OnConnected`.

```csharp
await connection.ConnectAsync();
```

It's as easy as that! The `OnConnected` event will be fired when the handshake is finished, but during that time the voice connection is still fully usable.

## Connection Invalidation
Unlike the gateway connection, voice connections do not automatically reconnect when a fatal error occurs. Therefore, when a voice connection disconnects (either normally or from an error) it is considered invalid. When this occurs the `OnInvalidated` event will be fired. You can check if a connection is invalid beforehand using the `IsValid` property.

When a `DiscordVoiceConnection` is invalid, any attempt to use it will result in a no-op instead of an exception being thrown.

Invalidation occurs during the following scenarios:
- Failed handshake
- Normal disconnection
- Any fatal error
- Disposing of the connection

## Handling Errors
When a fatal error occurs within a voice connection, the `OnError` event is fired with the details. When this occurs, the Discord user will automatically be removed from the voice channel and a new `DiscordVoiceConnection` will need to be created.

## Continuation
- [Sending Voice Data](./Sending-Voice-Data.md)