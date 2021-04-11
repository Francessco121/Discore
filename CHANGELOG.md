## v5.0.0
- `IDiscordGateway.OnMessageUpdated` now emits a `DiscordPartialMessage` instead of a normal `DiscordMessage`.
- Removed dependency on `Newtonsoft.Json`.

### Bug Fixes
- Fixed race condition that occurred when the application is kicked from a voice channel.
