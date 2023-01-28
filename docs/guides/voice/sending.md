# Sending Voice Data

## Setup

### Data Requirements
When sending voice data to Discord, the data must be in [PCM form](https://en.wikipedia.org/wiki/Pulse-code_modulation). A good tool that can convert many audio formats to PCM is [FFmpeg](https://ffmpeg.org/).

### Speaking State
Before voice data can be sent, the speaking state of the voice connection must be `true`. When voice data is finished being sent, this can be set back to `false` to indicate it's done.

This is set through the [`DiscordVoiceConnection.SetSpeakingAsync`](xref:Discore.Voice.DiscordVoiceConnection.SetSpeakingAsync*) method and can be checked through the [`IsSpeaking`](xref:Discore.Voice.DiscordVoiceConnection.IsSpeaking) property:
```csharp
// Assume variable voice is our connection.
DiscordVoiceConnection voice = ...;

// Set speaking if not already
if (!voice.IsSpeaking)
    await voice.SetSpeakingAsync(true);
```

## Sending Voice Data
Sending voice data in Discore is fairly simple. `DiscordVoiceConnection` contains two methods for this, [`SendVoiceData`](xref:Discore.Voice.DiscordVoiceConnection.SendVoiceData*) and [`CanSendVoiceData`](xref:Discore.Voice.DiscordVoiceConnection.CanSendVoiceData*). Instead of using a blocking stream to send the data, Discore allows the application to create their own system for sending data to the voice buffer within `DiscordVoiceConnection`.

`CanSendVoiceData` takes a byte amount and returns whether there is currently enough room in the voice data buffer for that amount of data. `SendVoiceData` can then be used to send the actual data.

The size of each voice data block sent to the connection should be at max [`DiscordVoiceConnection.PCM_BLOCK_SIZE`](xref:Discore.Voice.DiscordVoiceConnection.PCM_BLOCK_SIZE). Anything bigger can result in the audio playing faster than intended and/or introduce other audio artifacts. Sending smaller blocks can also create artifacts, but this is not always avoidable.

An example send-loop is [available below](#send-loop-example).

> [!TIP]
> It is **highly** recommended to perform the send-loop in a dedicated thread instead of using async/await. When performing delays to wait for room in the voice buffer, using `Task.Delay` can end up waiting significantly longer than expected due to how the task scheduler works. This can cause audio to pause intermittently. Instead, use `Thread.Sleep` and avoid async/await for better results. 

## Manipulating the Voice Buffer

### Getting Unsent Data and Clearing
The number of bytes that still need to be sent out to Discord can be retrieved with the [`BytesToSend`](xref:Discore.Voice.DiscordVoiceConnection.BytesToSend) property. If the application needs this buffer to be clear, the [`ClearVoiceBuffer`](xref:Discore.Voice.DiscordVoiceConnection.ClearVoiceBuffer*) method can be called to cancel any queued data.

```csharp
// Assume variable voice is our connection.
DiscordVoiceConnection voice = ...;

// Ensure voice buffer is empty.
if (voice.BytesToSend > 0)
    voice.ClearVoiceBuffer();
```

### Pausing/Resuming
The underlying loop of sending this queued data can also be paused if necessary with the [`IsPaused`](xref:Discore.Voice.DiscordVoiceConnection.IsPaused) property.

## Send-Loop Example
A simple send-loop example could be implemented as follows:
```csharp
// Assume variable voice is our connection.
DiscordVoiceConnection voice = ...;

// Create a buffer for moving data from the source to the voice connection.
byte[] transferBuffer = new byte[DiscordVoiceConnection.PCM_BLOCK_SIZE];

while (sendingVoiceData && voice.IsValid)
{
    // Check if there is room in the voice buffer
    if (voice.CanSendVoiceData(transferBuffer.Length))
    {
        // Read some voice data into our transfer buffer.
        //
        // Note: It may be beneficial to wait for the transferBuffer to
        // be full before sending data if the data source frequently reads
        // smaller blocks of data.
        int read = source.Read(transferBuffer, 0, transferBuffer.Length);
        // Send the data we read from the source into the voice buffer.
        voice.SendVoiceData(transferBuffer, 0, read);
    }
    else
    {
        // Wait for at least 1ms to avoid burning CPU cycles.
        Thread.Sleep(1);
    }
}
```

---
Next: [Gateway–Voice Bridges](./bridges.md)
