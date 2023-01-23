# Gateway–Voice Bridges
Sometimes, it is desirable to handle voice connections in a different process (or even on a different server) than the main bot application. Typically, this is due to voice connections requiring more system resources and/or dependencies.

Discore provides the abstraction [`IGatewayVoiceBridge`](xref:Discore.Voice.IGatewayVoiceBridge) to make this possible. A [`DiscordVoiceConnection`](xref:Discore.Voice.DiscordVoiceConnection) does not require an in-process `Shard` instance to function, it only needs to receive a couple Gateway events and needs the ability to send voice state updates.

> [!TIP]
> Only the application responsible for voice connections requires `libopus` and `libsodium` to be installed.

## Creating a Custom Bridge
Let's look at a simple example. We want to split our bot into two applications: one that handles main bot operations (i.e. Gateway connections) and one that handles exclusively voice connections. In this case, each application will need to communicate with each other. For simplicity, let's say that each application exposes a small networked API (such as an HTTP REST API) for this purpose.

### Forwarding Voice Events
The first step is forwarding voice events (specifically [`OnVoiceStateUpdate`](xref:Discore.WebSocket.IDiscordGateway.OnVoiceStateUpdate) and [`OnVoiceServerUpdate`](xref:Discore.WebSocket.IDiscordGateway.OnVoiceServerUpdate)) to the voice application. This can be done like so:
```csharp
// Get our Gateway connection.
IDiscordGateway gateway = ...;

// Get an API client to our voice application.
MyVoiceAppApi voiceAppApi = ...;

// Listen for voice events.
gateway.OnVoiceStateUpdate += OnVoiceStateUpdate;
gateway.OnVoiceServerUpdate += OnVoiceServerUpdate;

...

// Forward events through the API.
void OnVoiceStateUpdate(object? sender, VoiceStateUpdateEventArgs e)
{
    voiceAppApi.VoiceStateUpdate(e.VoiceState);
}

void OnVoiceServerUpdate(object? sender, VoiceServerUpdateEventArgs e)
{
    voiceAppApi.VoiceServerUpdate(e.VoiceServer);
}
```

### Receiving Voice Events
Next, let's create the actual `IGatewayVoiceBridge` implementation and let it receive these voice events. This will live in the voice application, not the main bot.
```csharp
public class MyGatewayVoiceBridge : IGatewayVoiceBridge
{
    public event EventHandler<BridgeVoiceStateUpdateEventArgs>? OnVoiceStateUpdate;
    public event EventHandler<BridgeVoiceServerUpdateEventArgs>? OnVoiceServerUpdate;

    readonly MyAppApi mainAppApi;

    public MyGatewayVoiceBridge(MyAppApi mainAppApi)
    {
        this.mainAppApi = mainAppApi;

        // Listen for API calls
        //
        // For the sake of example, we'll assume that our API client forwards API
        // requests to some C# events.
        mainAppApi.OnVoiceStateUpdate += Api_OnVoiceStateUpdate;
        mainAppApi.OnVoiceServerUpdate += Api_OnVoiceServerUpdate;
    }

    public async Task UpdateVoiceStateAsync(
        Snowflake guildId,
        Snowflake? channelId,
        bool isMute = false,
        bool isDeaf = false,
        CancellationToken? cancellationToken = null)
    {
        // Call back into the main app
        // ...
    }

    // Forward events through the bridge.
    void Api_OnVoiceStateUpdate(object? sender, DiscordVoiceState e)
    {
        OnVoiceStateUpdate?.Invoke(this, e);
    }

    void Api_OnVoiceServerUpdate(object? sender, DiscordVoiceServer e)
    {
        OnVoiceServerUpdate?.Invoke(this, e);
    }
}
```

### Putting It All Together
For brevity, sending voice state updates from the voice application back to the main bot will be left as an exercise to the reader. This involves simply calling [`IDiscordGateway.UpdateVoiceStateAsync`](xref:Discore.WebSocket.IDiscordGateway.UpdateVoiceStateAsync*) in the application that owns the Gateway connection.

All that's left is to start creating `DiscordVoiceConnection`s with our new bridge. This can be done as follows:
```csharp
// Get our bridge.
MyGatewayVoiceBridge bridge = ...;

// Get our bot's user ID.
Snowflake botUserId = ...;

// Get the voice channel to connect to.
Snowflake voiceChannelId = ...;

// Create a new voice connection.
var connection = new DiscordVoiceConnection(bridge, botUserId, voiceChannelId);
```

> [!TIP]
> The bot's user ID can be retreived from [`Shard.UserId`](xref:Discore.WebSocket.Shard.UserId).

From here, the `DiscordVoiceConnection` instance can be used like normal!

## Lavalink
While Discore does not currently provide a built-in [Lavalink](https://github.com/freyacodes/Lavalink) client, applications have the ability to integrate themselves.

Lavalink needs the following information from Discore:
- [The voice session ID](xref:Discore.Voice.DiscordVoiceState.SessionId).
- [The voice server token](xref:Discore.Voice.DiscordVoiceServer.Token).
- [The voice server endpoint](xref:Discore.Voice.DiscordVoiceServer.Endpoint).

Just like with the custom bridges described above, this information can be passed by forwarding the [`OnVoiceStateUpdate`](xref:Discore.WebSocket.IDiscordGateway.OnVoiceStateUpdate) and [`OnVoiceServerUpdate`](xref:Discore.WebSocket.IDiscordGateway.OnVoiceServerUpdate) Gateway events. Unlike bridge's within Discore, your application will need to initiate the connection, which can be done by calling [`IDiscordGateway.UpdateVoiceStateAsync`](xref:Discore.WebSocket.IDiscordGateway.UpdateVoiceStateAsync*).

Please see [Lavalink's implementation documentation](https://github.com/freyacodes/Lavalink/blob/master/IMPLEMENTATION.md) for more information.
