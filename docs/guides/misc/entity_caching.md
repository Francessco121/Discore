# Entity Caching
Out of the box, entities (like [`DiscordUser`](xref:Discore.DiscordUser)) are not cached in any way. These classes returned from the HTTP API and Gateway events are given as snapshots of the current state of that entity. Entity classes are immutable, so to get the updated state you will normally need to either query it from the HTTP API or wait for a Gateway event to send you the new state.

To alleviate this issue and help applications avoid making unnecessary API calls, Discore comes with the caching utility [`Discore.Caching.DiscordMemoryCache`](xref:Discore.Caching.DiscordMemoryCache). This class is capable of automatically caching entities returned from a specific shard/Gateway connection.

Utilizing this memory cache, applications can usually immediately get the most recent state of an entity that they need instead of waiting for an API call. For example, the state of a `DiscordGuild` is kept up-to-date locally in this cache (although the `DiscordGuild` class is still immutable and the cache may return a newer instance).

> [!NOTE]
> The `DiscordMemoryCache` only caches entities received through the Gateway and **not** through HTTP API calls.

> [!WARNING]
> Applications should take care when using the cache and **should never** assume that a given entity is available. The cache may be missing entities or even be empty for various reasons such as:
> - The entity hasn't been received through an event yet.
> - The shard just reconnected with a new session or is currently disconnected.

## Using the Memory Cache
First, create a new `DiscordMemoryCache` by giving it the shard that you wish to cache entities from:
```csharp
var cache = new DiscordMemoryCache(shard);
```

The `DiscordMemoryCache` class contains a bunch of methods that, for the most part, mimic `DiscordHttpClient`. For instance, to retrieve a guild:

```csharp
DiscordGuild? guild = cache.GetGuild(guildId);
```

The [full list of methods can be view here](xref:Discore.Caching.DiscordMemoryCache#methods).

The cache also contains some WebSocket specific data such as [`DiscordGuildMetadata`](xref:Discore.WebSocket.DiscordGuildMetadata) and [`DiscordVoiceState`](xref:Discore.Voice.DiscordVoiceState) instances. With this information, you can look up extra guild info such as the date that the bot joined the guild and get a list of users that are currently in a given voice channel. Of course this information is still available outside of the cache through their respective Gateway events.

### Clean Up
The lifetime of the cache is at most the same as the shard it's associated with. When you're finished with a `DiscordMemoryCache` instance, make sure to dispose of it:
```csharp
cache.Dispose();
```

This will clear the cache (and let instances that were cached be garbage collected) as well as unsubscribe from shard/Gateway events.

## Custom Caching
The `DiscordMemoryCache` is a bit opinionated and completely optional. If you want caching but need to/want to handle things differently, please feel free to use [the `DiscordMemoryCache` source code](https://github.com/Francessco121/Discore/tree/master/src/Discore/Caching) as a reference for creating your own caching solution. There's a bit more to caching entities, such as dealing with merging partial updates with the base full entity.

