[[← back]](./README.md)

# Gateway Caching

Each shard contains its own local memory cache of state of the Discord API available through `shard.Cache`. This contains lots of information not directly available through some objects, such as lists of channels in each guild.

### Immutability
Every object representing state in the Discord API (e.g. `DiscordUser`, `DiscordGuild`, etc.) is immutable. To get an object representing the most up-to-date state, it must be either retrieved from the HTTP API, or pulled from cache.

### Cache Structure
The cache is broken down into a tree, starting with `DiscoreCache`. Any object in the Discord API that contains nested data, for example a guild, has its own cache (e.g. `DiscoreGuildCache`). These "nested caches" contain a `Value` property which will return the most up-to-date state for the object it represents, as well as other properties for the data of any nested objects.

### Cache Clearing
The cache is cleared in two scenarios to avoid stale and/or non-updateable data from being available:
- Manually stopping a shard.
- If the shard's gateway connection starts a new session.

If the cache is cleared automatically, for example due to a new gateway session, the cache will be repopulated immediately with up-to-date data.

### Example: Message Handling
```csharp
shard.Gateway.OnMessageCreated += Gateway_OnMessageCreated;

...

private static void Gateway_OnMessageCreated(object sender, MessageEventArgs e)
{
    DiscordMessage message = e.Message;
    DiscoreCache cache = e.Shard.Cache;

    // Get the text channel the message was sent in, from cache and expect it to be a guild channel.
    DiscordGuildTextChannel guildTextChannel = cache.Channels.Get(message.ChannelId) as DiscordGuildTextChannel;
    if (guildTextChannel != null)
    {
        // Get the cache of the guild the channel is in.
        DiscoreGuildCache guildCache = cache.Guilds.Get(guildTextChannel.GuildId);

        // Get current state of guild.
        DiscordGuild guild = guildCache.Value;

        ...    
    }
    else { /* Message was sent from a DM. */}
}
``` 