[[← back]](./README.md)

# Shard Caches

Discore caches most of the data received by Discord's WebSocket API in an instance of `Discore.WebSocket.DiscordShardCache` available on a `Shard` through the `Cache` property.

This data can be used to view the state of an entity quickly without the need for an HTTP call. For instance, the state of a `DiscordGuild` is kept up-to-date locally in this cache, which means that applications do not always need to wait for `DiscordHttpClient.GetGuild`.

## Using the Cache
The `DiscordShardCache` class contains a set of methods that, for the most part, mimic `DiscordHttpClient`. For instance, to retrieve a guild:

```csharp
DiscordGuild guild = cache.GetGuild(guildId);
```

The cache also contains some WebSocket specific data such as `DiscordGuildMetadata` and `DiscordVoiceState` instances.

## Immutability
Every entity in the cache is 100% immutable. This means that applications should never store references to an entity retrieved from the cache for long periods of time, as the state may become out-dated.

## Volatility
Applications should take care when using the cache and **should never** assume that a given entity is available. Everytime a `Shard` is required to create a new session, its cache is completely cleared. This also means that the cache may contain nothing for a short period during a reconnection.
