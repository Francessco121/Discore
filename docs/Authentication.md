[[← back]](./README.md)

# Authentication

## Authenticators
Authenticators are implemented through the `IDiscordAuthenticator` interface. These authenticators are then passed to any new `DiscordWebSocketApplication` or `DiscordHttpApplication` instances.

### Bot User Token
If your application is using a bot user, its token can be used for authentication:
```csharp
string token = "bot user token goes here";

// Create authenticator using the bot user token method
DiscordBotUserToken authenticator = new DiscordBotUserToken(token);
```
