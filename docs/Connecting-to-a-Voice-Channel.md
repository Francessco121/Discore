[[← back]](./README.md)

# Connecting to a Voice Channel

Each shard contains a `ShardVoiceManager` available through `Shard.Voice`. The `ShardVoiceManager` keeps track of all open voice connections to guilds that are managed by that shard.

## Create a Voice Connection
```csharp
// Get the ID of the guild you wish to create a voice connection in.
Snowflake guildId = ...;
// Create a/get an existing connection.
DiscordVoiceConnection connection = shard.Voice.CreateOrGetConnection(guildId);
```

## Initiate the Connection
Creating a voice connection does not actually initiate the connection, this must be done manually. This is done manually to allow the application to safely subscribe to events like `DiscordVoiceConnection.OnConnected`.

```csharp
// Get the ID of the voice channel you wish to connect to.
Snowflake voiceChannelId = ...;

// Initiate the connection.
await connection.ConnectAsync(voiceChannelId);
```

It's as easy as that! The `OnConnected` event will be fired when the handshake is finished.

## Connection Invalidation
Unlike the gateway connection, voice connections do not automatically reconnect when a fatal error occurs. Therefore, when a voice connection disconnects (either normally or from an error) it is considered invalid. When this occurs the `OnInvalidated` event will be fired with a reason for the invalidation. You can check if a connection is invalid beforehand using the `IsValid` property.

When a `DiscordVoiceConnection` is invalid, any attempt to use it will result in a no-op instead of an exception being thrown.

Invalidation occurs during the following scenarios:
- Failed handshake
- Normal disconnection
- Any fatal error
- Disposing of the connection

## Continuation
For information on how to sendvoice data see:
- [Sending Voice Data](./Sending-Voice-Data)
