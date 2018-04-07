[[← back]](./README.md)

# 2.x to 3.x Migration Guide

## Pre-upgrade steps

### Change all code using deprecated 2.x functionality
All deprecated 2.x code has been removed in 3.0, making sure your application is not using any deprecated code will make the upgrade much smoother.

### Remove/change any code using Discore for non-bot API operations
Discore 3.0 removes all support for non-bot Discord applications. This means that operations such as accepting invites or accessing user connections (i.e. the `DiscordConnection` class) is no longer possible with Discore.

#### Why?
The Discord developers have made it fairly clear that user bots are not supported. Along with that, the API documentation has also shown that creating custom Discord clients is also really not supported. Because of this, Discore will only be supporting bots from here on.

##### What about accepting invites?
Discord bots cannot accept invites, the only API support for this is intended for applications accepting invites on behalf of an actual user. In most if not all scenarios, a bot's codebase will not be doing this anyway.

## Major changes

### DiscordHttpApplication and DiscordWebSocketApplication have been removed
It is no longer necessary to create these 'application wrappers' to use Discore. To replace this, shards and HTTP clients can be manually instantiated. Because of this, `ShardManager` has also been removed.

Using the HTTP API now looks like:
```csharp
DiscordHttpClient httpClient = new DiscordHttpClient(token);
```

Using the WebSocket API now looks like:
```csharp
Shard shard = new Shard(token, shardId, totalShards);
```

Each of these can now be created and destroyed as needed, without being tied to the original application wrapper classes.

For more information on using these two classes in 3.x, see the [shard](./Shards.md) and [HTTP client](./HTTP-Clients.md) wiki pages.

### The shard cache has been completely rewritten
`DiscoreCache` has been rewritten and renamed to `DiscordShardCache`. Along with this, structural changes have been made. 

"Parent" caches such as `DiscoreGuildCache` and `DiscoreMemberCache` have been removed. For example, obtaining a guild member no longer looks like:

```csharp
if (cache.Guilds.TryGetValue(guildId, out DiscoreGuildCache guildCache))
{
    DiscordGuildMember member = guildCache.Members[userId].Value;
}
```

And now looks like:

```csharp
DiscordGuildMember member = cache.GetGuildMember(guildId, userId);
```

In addition, the cache no longer contains collections of each stored type. Instead the cache's API looks similar to the HTTP API. For example:

```csharp
var guild = cache.Guilds[guildId];
```

Now looks like:

```csharp
DiscordGuild guild = cache.GetGuild(guildId);
```

For more information on using the new 3.x cache, see the [shard cache](./Shard-Caches.md) wiki page.

#### Cache side-note
It is highly recommended that applications avoid using the cache unless an entity's state is required. For example, you don't need an `ITextChannel` instance to send a message. The HTTP API should be favored when possible for simplicity.


### The HTTP API implementation has been flattened
The original implementation split each API section across different classes. For instance, 2.x code would look like:

```csharp
await http.Channels.Get(id);
```

In 3.0, these separate classes have been removed. This changes the above example to:

```csharp
await http.GetChannel(id);
```

### Any HTTP method that returned a boolean is now void
Originally, any HTTP endpoint that returned a 204 no content on success resulted in the following Discore method returning a boolean indicating success. This is pointless however, since a `DiscordHttpApiException` is thrown if the request failed.

For instance:
```csharp
public Task<bool> DeleteMessage(Snowflake channelId, Snowflake messageId)
```

Is now:
```csharp
public Task DeleteMessage(Snowflake channelId, Snowflake messageId)
```

### Many HTTP error code enum value names have been shortened
For the sake of not typing ungodly things like `DiscordHttpErrorCode.MessagesCanOnlyBePinnedInTheChannelItWasCreated`, most enum values have been shortened:

Old                                          | New
---------------------------------------------|---------
BotsCannotUseThisEndpoint                    | BotsNotAllowed
OnlyBotsCanUseThisEndpoint                   | OnlyBotsAllowed
CannotExecuteActionOnDMChannel               | InvalidDMChannelAction
CannotEditMessageByOtherUser                 | InvalidMessageAuthorEdit
CannotSendEmptyMessage                       | MessageEmpty
CannotSendMessagesToUser                     | CannotMessageUser
CannotSendMessagesInVoiceChannel             | CannotMessageVoiceChannel
ChannelVerificationLevelTooHigh              | ChannelVerificationError
OAuth2ApplicationDoesNotHaveBot              | OAuth2AppMissingBot
OAuth2ApplicationLimitReached                | OAuth2AppLimitReached
NoteIsTooLong                                | NoteTooLong
ProvidedTooFewOrTooManyMessagesToDelete      | InvalidBulkDelete
MessagesCanOnlyBePinnedInTheChannelItWasCreated | InvalidMessagePin 
CannotExecuteActionOnASystemMessage          | InvalidMessageTarget 
AMessageProvidedWasTooOldToBulkDelete        | InvalidBulkDeleteMessageAge


### HTTP rate limiting methods have been condensed into one
Applications can no longer choose the way HTTP rate limits are dealt with. Each option has been replaced by a single correct implementation.


### Removed DiscordVoiceConnection OnError and OnDisconnected events
`DiscordVoiceConnection.OnError` and `DiscordVoiceConnection.OnDisconnected` have both been removed in favor of the existing `OnInvalidated` event. Because of this, `OnInvalidated` now carries the reason for the invalidation.


### Builder classes have been renamed for consistency
Builder class names were all over the place in 2.x. All of these have been renamed to fit the pattern "Object/Operation Name" + "Options".

Old                           | New
------------------------------|----------
DiscordMessageEdit            | EditMessageOptions 
DiscordMessageDetails         | CreateMessageOptions 
DiscordEmbedBuilder           | EmbedOptions 
PositionParameters            | PositionOptions
OverwriteParameters           | OverwriteOptions
ModifyRoleParameters          | ModifyRoleOptions
ModifyIntegrationParameters   | ModifyIntegrationOptions 
ModifyGuildParameters         | ModifyGuildOptions 
ModifyGuildMemberParameters   | ModifyGuildMemberOptions 
ModifyGuildEmbedParameters    | ModifyGuildEmbedOptions 
GuildVoiceChannelParameters   | GuildVoiceChannelOptions 
GuildTextChannelParameters    | GuildTextChannelOptions 
ExecuteWebhookParameters      | ExecuteWebhookOptions 
CreateRoleParameters          | CreateRoleOptions 
CreateGuildRoleParameters     | CreateGuildRoleOptions 
CreateGuildParameters         | CreateGuildOptions 
CreateGuildChannelParameters  | CreateGuildChannelOptions
 

### DiscoreLogger can no longer be instantiated
Due to potential major changes being brought to the internal logging system of Discore, `DiscoreLogger` can no longer be instantiated outside of Discore. Applications should be using their own logging methods anyway.

**Note:** This does **not** remove `DiscoreLogger.OnLog`. Internal logs can still be viewed outside of Discore for debugging purposes.