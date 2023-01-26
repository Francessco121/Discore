# Discore
[![NuGet](https://img.shields.io/nuget/v/Discore.svg?style=flat-square)](https://www.nuget.org/packages/Discore/)

Discore is a light-weight .NET library for creating [Discord](https://discord.com/) bots.

> **Please note:** Discore is **not** an official Discord client library!

The goal of Discore is to provide a minimal interface to Discord's APIs and to let applications decide the best way to interact with Discord. Discore takes care of all of the technical details required to use Discord's APIs such as connection management, WebSocket protocols, voice UDP protocols, rate limiting, authentication, etc.

Applications using Discore have access to:
- Each individual HTTP API route.
- Hooks for each real-time WebSocket Gateway event.
- Voice connections and the ability to send voice data to them.

## Documentation
Documentation for the latest stable release can be found [here on GitHub](https://github.com/Francessco121/Discore/wiki).

## Downloading
Releases are available through [NuGet](https://www.nuget.org/packages/Discore/). These are published alongside a [GitHub release](https://github.com/Francessco121/Discore/releases), which contains the fully detailed change log.

## Alternatives
Don't like our approach? Try some other great options:
- [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus)
- [Discord.Net](https://github.com/discord-net/Discord.Net)
