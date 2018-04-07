[[← back]](./README.md)

# Voice Prerequisites

In order to send/receive voice data through Discore, two external libraries are needed; [libsodium](https://download.libsodium.org/doc/) and [opus](http://opus-codec.org/).

Discore expects the libsodium and opus binaries to be named `libsodium` and `opus` respectively with the OS specific extension and/or prefix. For example, on Windows the binaries would need to be named `libsodium.dll` and `opus.dll`.

## Adding to Visual Studio 2017 Projects
**Note:** This is only necessary if `libsodium` or `opus` are not available globally on the target system.

Assuming this is a .NET Core project, the binaries need to be dropped in the same directory as the `.csproj`. When publishing the project, the binaries need to be in the same directory as the application binary.

## Pre-Compiled Windows Binaries
If your application is targeting Windows, the Discore repository contains tested pre-compiled 32 and 64-bit binaries [which can be downloaded here](https://github.com/BundledSticksInkorperated/Discore/tree/v2/lib/windows).

## Using the Discore Voice API
Once everything is all setup, please see [the documentation on how to use the voice API here](./Connecting-to-a-Voice-Channel.md).