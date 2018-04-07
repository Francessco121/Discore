[[← back]](./README.md)

# HTTP Clients

To access the Discord HTTP API through Discore, an instance of `Discore.Http.DiscordHttpClient` is needed.

## Creating an HTTP Client

Applications can create as many instances of `DiscordHttpClient` as needed, the only requirement is the bot's user token:

```csharp
DiscordHttpClient httpClient = new DiscordHttpClient(TOKEN);
```

## Using the HTTP Client
Every supported HTTP endpoint is available as a method on `DiscordHttpClient` named consistently with the actual endpoint's name documented [in Discord's API documentation](https://discordapp.com/developers/docs/intro).

For example, to use the [create message](https://discordapp.com/developers/docs/resources/channel#create-message) endpoint:

```csharp
await httpClient.CreateMessage(channelId, "Hello World!");
```

## Handling API Errors
If a request happens to trigger an API error, a `DiscordHttpApiException` will be thrown. This exception contains the Discord HTTP API error code, as well as the HTTP status code associated with the response.

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