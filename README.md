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
Documentation for the latest stable release can be found online:
- [Discore Docs](https://francessco.us/Discore/)
    - [Getting started](https://francessco.us/Discore/guides/getting_started.html)
    - [API documentation](https://francessco.us/Discore/api/index.html)
    - [Changelog](https://francessco.us/Discore/updates.html)

Sample applications can be found under [the `samples` directory](https://github.com/Francessco121/Discore/tree/v5/samples) in this repository.

To view or edit the documentation offline, please see the wiki for [building the docs site](https://github.com/Francessco121/Discore/wiki/Building-the-docs-site). 

## Issues/Questions
If you run into a bug, would like to request a feature, or just have a question, please feel free to [open an issue](https://github.com/Francessco121/Discore/issues) or [start a discussion](https://github.com/Francessco121/Discore/discussions). Please prefer the discussions section for questions.

## Contributing
Contributions are welcome! For small changes, you can just [open a pull request](https://github.com/Francessco121/Discore/pulls). However, for any larger changes please open a related issue (if there isn't one already) to discuss the proposed changes. PRs with more significant changes are less likely to be merged without some prior discussion.
