## v5.0.0
### Breaking Changes
- Discore now targets .NET Standard 2.1.
- Discord entities (such as `DiscordMessage`) no longer keep a reference to a `DiscordHttpClient` internally and therefore no longer have methods for modifying the entity.
    - All removed methods were just shortcuts for `DiscordHttpClient` calls and can be migrated by just using a `DiscordHttpClient` instead (ex. `DiscordMessage.Edit()` -> `DiscordHttpClient.EditMessage(DiscordMessage)`).
- Caching breaking changes:
    - `DiscordShardCache` has been replaced with the new `Discore.Caching.DiscordMemoryCache`.
    - `Shard.Cache` was removed in favor of making caching optional for Gateway connections. To get built-in caching behavior back, instantiate the new `DiscordMemoryCache` with your `Shard`. The new `DiscordMemoryCache` object has a nearly identical interface to the old `DiscordShardCache`.
    - DM channels are no longer cached since the Gateway no longer sends 'Channel Create' events for DMs as of v8.
- `IDiscordGateway` breaking changes:
    - Events have been renamed to be more consistent. All new event names follow the pattern `OnEventName` (e.g. `OnMessageCreated` was renamed to `OnMessageCreate`).
    - Events now emit unique event args per event instead of some events sharing arg types (arg class names now follow the pattern `EventNameEventArgs`).
    - `OnMessageUpdated` now emits a `DiscordPartialMessage` instead of a normal `DiscordMessage`.
    - `OnGuildAvailable` and `OnGuildUnavailable` were merged into `OnGuildCreate` and `OnGuildDelete`. To see whether the guild availability status changed, use the new `BecameAvailable` and `Unavailable` properties in the event args.
    - `OnDMChannel*` events were removed (bots no longer receive events for DM channels as of Gateway v8).
    - `OnChannel*` events now emit a `DiscordChannel` instead of a `DiscordGuildChannel` (matches the actual Gateway spec).
- `Shard.StartAsync` now requires either a `GatewayIntent` or `ShardStartConfig` argument.
- Renamed `DiscordGame` to `DiscordActivity` (to match Gateway v8 changes).
- Renamed `DiscordGameType` to `DiscordActivityType` (to match Gateway v8 changes).
- Removed `DiscordUserPresence.Game` (use `Activities.FirstOrDefault()` instead).
- `DiscordVoiceConnection.SetSpeakingAsync` will now throw an `InvalidOperationException` if called before being fully connected.
- `DiscordVoiceConnection.ClearVoiceBuffer` will now throw an `InvalidOperationException` if called before being fully connected.
- `DiscordVoiceConnection.ConnectAsync` no longer checks if the application is allowed to join the voice channel. If the application is not allowed to join the connection will still fail, but it will be attempted.
    - To get this functionality back, please use the new `DiscordPermissionHelper.CanJoinVoiceChannel` helper function.
- `DiscordHttpRateLimitException.Limit` is now nullable.
- `DiscordHttpRateLimitException.Reset` is now nullable.
- `DiscordHttpRateLimitException.ResetHighPrecision` is now nullable.
- `ShardStartConfig.GatewayLargeThreshold` now defaults to the Gateway's default of 50 (down from 250).
- `DiscordHttpErrorCode.TooManyReactions` renamed to `MaximumReactionsReached`.
- `DiscordHttpRateLimitException.RetryAfter` is now a `double` and includes millisecond precision.
- Guild embeds are replaced with guild widgets (API v8 change):
    - Removed `DiscordGuild.IsEmbedEnabled` and `EmbedChannelId` (use `IsWidgetEnabled` and `WidgetChannelId` instead).
    - Renamed `DiscordGuildEmbed` to `DiscordGuildWidgetSettings`.
    - Renamed `DiscordHttpErrorCode.EmbedDisabled` to `WidgetDisabled`.
    - Renamed `ModifyGuildEmbedOptions` to `ModifyGuildWidgetOptions`.
    - Renamed `DiscordHttpClient.GetGuildEmbed` to `GetGuildWidget`.
    - Renamed `DiscordHttpClient.ModifyGuildEmbed` to `ModifyGuildWidget`.
- Removed deprecated `ShardFailureReason.IOError`
- `DiscordHttpClient.BeginGuildPrune` now returns null if `computePruneCount` is true.
- Renamed `Create/EditMessageOptions.Embed` to `Embeds` to support multiple embeds permessage.
- `DiscordHttpClient.Create/EditMessage` file attachment overloads have been removed. Instead, please use the new `Attachments` property of `Create/EditMessageOptions`.

### Additions
- Added support for creating and editing messages with multiple embeds and/or multiple attachments.
- Added support for Gateway intents.
    - Intents can be specified using `ShardStartConfig.Intents` or `Shard.StartAsync(GatewayIntent)`.
- Added `Create/EditMessageOptions.Flags`.
- Added `DiscordAttachment.Description`.
- Added `DiscordAttachment.ContentType`.
- Added `DiscordAttachment.Ephemeral`.
- Added `Shard.OnDisconnected`.
- Added `ShardFailureReason.InvalidIntents`.
- Added `ShardFailureReason.DisallowedIntents`.
- Added `DiscordHttpApiException.Errors`.
- Added `DiscordHttpRateLimitException.Bucket`.

### Changes
- Discore now makes full use of C# 8 null safety!
- `Snowflake` now implements `IEquatable<Snowflake>`.
- `DiscordCdnUrl` now implements `IEquatable<DiscordCdnUrl>`.
- Removed dependency on `Newtonsoft.Json`.

### Bug Fixes
- Fixed race condition that occurred when the application is kicked from a voice channel.
- Fixed `DiscordWebSocketException` not containing an `InnerException` when one was provided.
- Internal bug fixes found thanks to null safety.
