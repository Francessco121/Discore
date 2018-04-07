[[← back]](./README.md)

# Sending Voice Data

## Setup

### Data Requirements
When sending voice data to Discord, the data must be in [PCM form](https://en.wikipedia.org/wiki/Pulse-code_modulation). A good tool that can convert many audio formats to PCM is [ffmpeg](https://ffmpeg.org/).

### Speaking State
Before voice data can be sent, the speaking state of the voice connection must be `true`. This will cause the user to appear as speaking. When voice data is finished being sent, this can be set back to `false` to indicate it's done.

This is set through the `DiscordVoiceConnection.SetSpeakingAsync` method and can be checked through the `IsSpeaking` property:
```csharp
// Assume variable voice is our connection.
DiscordVoiceConnection voice = ...;

// Set speaking if not already
if (!voice.IsSpeaking)
    await voice.SetSpeakingAsync(true);
```

## Sending Voice Data
Sending voice data in Discore is fairly simple. `DiscordVoiceConnection` contains two methods for this, `SendVoiceData` and `CanSendVoiceData`. Instead of using a blocking stream to send the data, we allow the application to create their own system for sending data to the voice buffer in `DiscordVoiceConnection`.

`CanSendVoiceData` takes a byte amount and returns whether there is enough room in the voice data to send that amount of data. `SendVoiceData` can then be used to send the actual data.

The size of each voice data block sent to the connection should be at max `DiscordVoiceConnection.PCM_BLOCK_SIZE`. Anything bigger can result in the audio playing faster than intended and/or introduce other audio artifacts. Sending smaller blocks can also create artifacts, but this is not always avoidable.

An example send-loop is [available below](#send-loop-example).

## Manipulating the Voice Buffer

### Getting Unsent Data and Clearing
The number of bytes that still need to be sent out to Discord can be retrieved with the `BytesToSend` property. If the application needs this buffer to be clear, the `ClearVoiceBuffer` method can be called to cancel any queued data.

```csharp
// Assume variable voice is our connection.
DiscordVoiceConnection voice = ...;

// Ensure voice buffer is empty.
if (voice.BytesToSend > 0)
    voice.ClearVoiceBuffer();
```

### Pausing/Resuming
The underlying loop of sending this queued data can also be paused if necessary with the `IsPaused` property.

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
        int read = source.Read(transferBuffer, 0, transferBuffer.Length);
        // Send the data we read from the source into the voice buffer.
        voice.SendVoiceData(transferBuffer, 0, read);
    }
    else
        // Sleep for at least 1ms to avoid burning CPU cycles.
        Thread.Sleep(1);
}
```

See the [connecting documentation](./Connecting-to-a-Voice-Channel.md) for information on how to create the `DiscordVoiceConnection`.