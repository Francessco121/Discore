[[← back]](./README.md)

# HTTP Applications

If your Discord application does not require realtime interaction with the Discord API or does not have a bot user, Discore can be used as a simple interface with the Discord HTTP API.

To use Discore for just HTTP API usage, simply:
```csharp
// Create an HTTP application
DiscordHttpApplication app = new DiscordHttpApplication(authenticator);
// Get the Discord HTTP API interface for the Discord application specified above
DiscordHttpApi httpApi = app.HttpApi;
```

For creating the `authenticator`, see the [authentication documentation](./Authentication).

## HTTP API Configuration
`DiscordHttpApplication` and `DiscordWebSocketApplication` both optionally take an `InitialHttpApiSettings` argument. `InitialHttpApiSettings` lets you change the way Discore works with the Discord HTTP API.

### Options In `InitialHttpApiSettings`
#### `RetryWhenRateLimited`
Default: `true`.
Sets whether rate limited requests should be resent. Additionally, this can be changed after an application is created by setting `DiscordHttpApi.RetryWhenRateLimited`.

#### `RateLimitHandlingMethod`
Default: `RateLimitHandlingMethod.Throttle`.
Sets how rate limits should be handled locally, options are:
- `Throttle`: Prevents being rate limited by placing requests into a queue and only sending them out when it is safe. Avoids 429's 99% of the time. **Note**: the queue is per rate-limited route, instead of effecting every type of request globally.
- `Minimal`: Doesn't try to control requests, but forces requests to wait after a 429 is received.

#### `UseSingleHttpClient`
Default: `true`.
Sets whether a single `HttpClient` should be used for all HTTP API requests, or if a new one should be created for each request. In rare cases, using a single `HttpClient` causes some requests to never send and end up with the task being cancelled after the `HttpClient`'s timeout time passes. We believe this is a .NET Core bug, as it seems to be operating system specific.

## HTTP API Interface
The `DiscordHttpApi` object provided from either a `DiscordHttpApplication` or a `DiscordWebSocketApplication` provides a simple interface with the Discord API that matches the way the HTTP API was broken down in [Discord's API documentation](https://discordapp.com/developers/docs/intro) (referring to the "resources" section).

For example, to get a user:
```csharp
DiscordHttpApi httpApi = app.HttpApi;
// User HTTP endpoints are specified under the "User" section in the Discord documentation
DiscordUser user = await httpApi.Users.Get(userId);
```

## Handling API Errors
If a request happens to trigger an API error, a `DiscordHttpApiException` will be thrown from the interface method. This exception contains the Discord HTTP API error code, as well as the HTTP status code associated with the response.

Example usage:
```csharp
try
{
    DiscordUser user = await httpApi.Users.Get(userId);
}
catch (DiscordHttpApiException apiEx)
{
    if (apiEx.ErrorCode == DiscordHttpErrorCode.UnknownUser)
    {
        // The user we are looking for does not exist :(
    }
}
```