# HTTP Client
To access the Discord HTTP API, an instance of [`Discore.Http.DiscordHttpClient`](xref:Discore.Http.DiscordHttpClient) is needed.

## Creating an HTTP Client
Applications can create as many instances of `DiscordHttpClient` as needed, the only requirement is the bot's user token:

```csharp
var httpClient = new DiscordHttpClient(TOKEN);
```

## Using the HTTP Client
Every supported API route is available as a method on `DiscordHttpClient` named consistently with the actual endpoint's name as documented [in Discord's API documentation](https://discord.com/developers/docs/intro). Additionally, you can [browse each method here](xref:Discore.Http.DiscordHttpClient#methods).

For example, to use the [Create Message](https://discord.com/developers/docs/resources/channel#create-message) endpoint:

```csharp
await httpClient.CreateMessage(channelId, "Hello World!");
```

## Handling API Errors
If a request happens to trigger an API error, a [`DiscordHttpApiException`](xref:Discore.Http.DiscordHttpApiException) will be thrown. This exception contains the Discord HTTP API error code and the HTTP status code associated with the response.

Example usage:
```csharp
try
{
    DiscordUser user = await httpClient.GetUser(userId);
}
catch (DiscordHttpApiException apiEx)
{
    if (apiEx.ErrorCode == DiscordHttpErrorCode.UnknownUser)
    {
        // The user we are looking for does not exist :(
    }
}
```

## Clean Up
When you're done with an `DiscordHttpClient` instance, make sure to dispose of it:
```csharp
httpClient.Dispose();
``` 

## Rate Limiting
Discord imposes rate limits on all HTTP API calls. Normally, Discore handles this for you and will automatically retry/delay requests internally to avoid hitting rate limits. However, if too many retries are attempted then a [`DiscordHttpRateLimitException`](xref:Discore.Http.DiscordHttpRateLimitException) may eventually be thrown.

You may also optionally disable automatic retries for rate limit errors by setting [`DiscordHttpClient.RetryWhenRateLimited`](xref:Discore.Http.DiscordHttpClient.RetryWhenRateLimited) to `false`. However, Discore will still attempt to delay requests to respect rate limiting even if this option is off. It's recommended to keep this option on unless you have a very good reason.

Situations where rate limit retries generally occur are: 
- You have multiple processes accessing the API with the same token (Discore coordinates rate limiting across all `DiscordHttpClient`s within the same process).
- Very specific timing errors (which is most likely a bug or an issue with the system clock).

In most if not every other case, Discore will delay requests sufficiently to avoid ever getting a rate limit error in the first place.
