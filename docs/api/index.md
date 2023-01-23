# API Documentation
Discore .NET API documentation generated directly from source code.

Namespaces:
- [Discore](xref:Discore): Contains all common classes such as Discord entities (i.e. [DiscordGuild](xref:Discore.DiscordGuild)) and utilities (i.e. [Snowflake](xref:Discore.Snowflake)).
- [Discore.Caching](xref:Discore.Caching): Optional entity caching utilities for shards. See [DiscordMemoryCache](xref:Discore.Caching.DiscordMemoryCache).
- [Discore.Http](xref:Discore.Http): Classes for interacting with Discord's HTTP API. See [DiscordHttpClient](xref:Discore.Http.DiscordHttpClient).
- [Discore.Voice](xref:Discore.Voice): Provides support for creating [DiscordVoiceConnection](xref:Discore.Voice.DiscordVoiceConnection)s and sending voice data to them.
- [Discore.WebSocket](xref:Discore.WebSocket): Lets bots connect to Discord's real-time WebSocket Gateway via [Shard](xref:Discore.WebSocket.Shard)s.

Get started by instantiating an entrypoint into Discore:
- [DiscordHttpClient](xref:Discore.Http.DiscordHttpClient): For working with Discord's HTTP API.
- [Shard](xref:Discore.WebSocket.Shard): For working with Discord's real-time WebSocket Gateway.
