# Discore.Samples.VoiceSending
This sample provides an example implementation of a bot that plays audio in voice channels. This could be used as the base for creating a music bot!

> **Note:** This sample makes use of [FFmpeg](https://ffmpeg.org/) to convert audio to PCM.

## Setup

### 1. Install libopus and libsodium
Discore requires [libopus](https://opus-codec.org/) and [libsodium](https://doc.libsodium.org/) to send voice data. Please follow the Discore [instructions for installing voice dependencies](https://francessco.us/Discore/guides/voice/prerequisites.html) for your operating system.

#### Note for Windows users
The `.csproj` for this sample contains MSBuild entries to copy `opus.dll` and `libsodium.dll` to the `bin` directory automatically for you. Instead of installing each DLL into the system path, you may place them in this directory.

### 2. Install FFmpeg
This sample requires the `ffmpeg` executable to be available. Linux users can download this from their operating system's package manager, Windows users can [download it from the FFmpeg website](https://ffmpeg.org/download.html), and macOS users can download it from Homebrew.

#### Note for Windows users
Just like the opus and libsodium DLLs, the `.csproj` for this sample can automatically copy `ffmpeg.exe` to the `bin` directory. Just place `ffmpeg.exe` in this directory.

### 3. Add your bot token
Create a file named `TOKEN.txt` in this directory containing your bot's token.

> **Note:** Your bot must have the `MessageContent` intent enabled in the [developer portal](https://discord.com/developers/applications)!

## Usage
This sample bot responds to 4 commands:
- `!join` - Tells the bot to join the voice channel that the invoking user is currently in.
- `!leave` - Tells the bot to leave the current voice channel that it is in.
- `!play <uri>` - Tells the bot to begin streaming the audio from the specified URI in the voice channel. The URI must be supported by FFmpeg.
- `!stop` - Tells the bot to stop streaming audio if it currently is.
