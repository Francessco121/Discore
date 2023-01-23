# Voice Prerequisites
To use voice connections, Discore requires two external libraries: [libsodium](https://doc.libsodium.org/) and [libopus](https://opus-codec.org/). libopus is used to encode audio into the Opus format and libsodium is used to encrypt the audio data for Discord.

Discore uses .NET's [P/Invoke](https://learn.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke) feature to use these libraries and will attempt to load them under their platform-specific names. The exact name and placement of these library files depends on your operating system's dynamic library loader. In general, these can either be in the working directory of your application or somewhere else on the system path.

## libopus

### [Linux](#tab/libopus-linux)
Discore expects the libopus library to be loadable as `libopus.so.0` on Linux (exact name may vary depending on dynamic library loader rules).

libopus can be downloaded either from your OS's package manager or be compiled [from source](https://opus-codec.org/downloads/).

### [Windows](#tab/libopus-windows)
Discore expects the libopus library to be named `opus.dll` on Windows.

Official Windows builds of libopus unfortunately don't seem to exist, so you will need to compile [from source](https://opus-codec.org/downloads/). At the time of writing, this requires at least Visual Studio 2015 with C compilation support. Within the libopus repository you will find `win32/VS2015/opus.sln`. Opening this in Visual Studio and compiling as `ReleaseDLL` for `x64` will emit the `opus.dll` file you need.

### [macOS](#tab/libopus-mac)
Discore expects the libopus library to be loadable as `libopus` on macOS (exact name may vary depending on dynamic library loader rules).

libopus can be downloaded via Homebrew or be compiled [from source](https://opus-codec.org/downloads/).

---

## libsodium

### [Linux](#tab/libsodium-linux)
Discore expects the libsodium library to be loadable as `libsodium` on Linux (exact name may vary depending on dynamic library loader rules).

libsodium can be downloaded either from your OS's package manager, from or be compiled [from source](https://download.libsodium.org/libsodium/releases/).

### [Windows](#tab/libsodium-windows)
Discore expects the libsodium library to be named `libsodium.dll` on Windows.

libsodium can be downloaded as [pre-built binaries](https://download.libsodium.org/libsodium/releases/) (the `*-msvc` downloads) or be compiled [from source](https://download.libsodium.org/libsodium/releases/) (the source tarball downloads).

### [macOS](#tab/libsodium-mac)
Discore expects the libsodium library to be loadable as `libsodium` on macOS (exact name may vary depending on dynamic library loader rules).

libsodium can be downloaded via Homebrew or be compiled [from source](https://download.libsodium.org/libsodium/releases/).

---

---
Next: [Voice Connections](./connections.md)
