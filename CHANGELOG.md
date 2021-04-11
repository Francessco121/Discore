## v5.0.0
### Breaking Changes
- Discore now targets .NET Standard 2.1.
- `DiscordHttpRateLimitException.Limit` is now nullable.
- `DiscordHttpRateLimitException.Reset` is now nullable.
- `DiscordHttpRateLimitException.ResetHighPrecision` is now nullable.
- `IDiscordGateway.OnMessageUpdated` now emits a `DiscordPartialMessage` instead of a normal `DiscordMessage`.
- `DiscordVoiceConnection.SetSpeakingAsync` will now throw an `InvalidOperationException` if called before being fully connected.
- `DiscordVoiceConnection.ClearVoiceBuffer` will now throw an `InvalidOperationException` if called before being fully connected.

### Other Changes
- Discore now makes full use of C# 8 null safety!
- `Snowflake` now implements `IEquatable<Snowflake>`.
- Removed dependency on `Newtonsoft.Json`.

### Bug Fixes
- Fixed race condition that occurred when the application is kicked from a voice channel.
- Fixed `DiscordWebSocketException` not containing an `InnerException` when one was provided.
- Internal bug fixes found thanks to null safety.
