using System;
using System.Threading;

namespace Discore.Audio
{
    /// <summary>
    /// An interface for interacting with a voice connection to Discord.
    /// </summary>
    public class DiscordVoiceClient : IDisposable
    {
        /// <summary>
        /// Called when the voice connection finishes connecting to Discord.
        /// </summary>
        public event EventHandler<VoiceClientEventArgs> OnConnected;
        /// <summary>
        /// Called when the voice session is disconnected.
        /// </summary>
        public event EventHandler<VoiceClientEventArgs> OnDisconnected;
        /// <summary>
        /// Called when the voice connection unexpectedly closes.
        /// </summary>
        public event EventHandler<VoiceClientExceptionEventArgs> OnFatalError;

        /// <summary>
        /// The <see cref="DiscordGuild"/> this voice client is connected to.
        /// </summary>
        public DiscordGuild Guild { get; }
        /// <summary>
        /// The <see cref="DiscordGuildMember"/> this client is communicating through.
        /// </summary>
        public DiscordGuildMember GuildMember { get { return voiceSocket.Member; } }
        /// <summary>
        /// The <see cref="DiscordClient"/> that created this voice client.
        /// </summary>
        public DiscordClient Client { get; }

        /// <summary>
        /// Current number of queued bytes not yet sent.
        /// </summary>
        public int BytesToSend { get { return audioBuffer.Count; } }

        /// <summary>
        /// Gets whether or not this <see cref="DiscordVoiceClient"/> is available to use.
        /// </summary>
        public bool IsValid { get { return !isDisposed; } }
        /// <summary>
        /// Gets whether or not this <see cref="DiscordVoiceClient"/> is connected to a <see cref="DiscordGuild"/>.
        /// </summary>
        public bool IsConnected { get { return isConnected && !isDisposed; } }

        // TODO: Make prebuffer configuration available publically
        const int PCM_BLOCK_SIZE = 3840;
        const int AUDIO_FRAMES_TO_PREBUFFER = 64;
        const int AUDIO_FRAMES_TO_BUFFER = 96;
        const int AUDIO_BUFFER_SIZE = PCM_BLOCK_SIZE * AUDIO_FRAMES_TO_BUFFER;

        DiscordLogger log;

        CircularBuffer audioBuffer;
        VoiceSocket voiceSocket;
        bool isDisposed;
        bool isSpeaking;
        bool isFlushing;
        bool isConnected;
        bool gotFirstBlock;
        Thread audioSendThread;

        internal DiscordVoiceClient(DiscordClient client, DiscordGuild guild)
        {
            Client = client;
            Guild = guild;

            log = new DiscordLogger($"DiscordVoiceClient:{guild.Name}");

            audioBuffer = new CircularBuffer(AUDIO_BUFFER_SIZE);

            audioSendThread = new Thread(AudioSendLoop);
            audioSendThread.Name = $"{log.Prefix} Audio Send Thread";
            audioSendThread.IsBackground = true;
            audioSendThread.Start();
        }

        internal void SetSocket(VoiceSocket socket)
        {
            voiceSocket = socket;
            voiceSocket.OnFatalError += VoiceSocket_OnFatalError;

            // Set speaking could have been called before the
            // socket arrived, so set it based on the last call.
            voiceSocket.SetSpeaking(isSpeaking);

            isConnected = true;

            log.LogVerbose("VoiceSocket received");

            OnConnected?.Invoke(this, new VoiceClientEventArgs(this));
        }

        private void VoiceSocket_OnFatalError(object sender, Exception e)
        {
            Disconnect();
            OnFatalError?.Invoke(this, new VoiceClientExceptionEventArgs(this, e));
        }

        /// <summary>
        /// Queues a section of PCM voice data to be sent.
        /// </summary>
        /// <param name="data">The array of voice data to read from.</param>
        /// <param name="offset">Where to start reading from the voice data array.</param>
        /// <param name="count">How many bytes to read from the voice data array.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the number of bytes to be queued 
        /// would exceed the buffer size of this <see cref="DiscordVoiceClient"/>.</exception>
        /// <remarks>
        /// Should be used along side <see cref="CanSendVoiceData(int)"/>
        /// to ensure the voice buffer is not overflowed.
        /// </remarks>
        public void SendVoiceData(byte[] data, int offset, int count)
        {
            if (audioBuffer.MaxLength - audioBuffer.Count < count)
                throw new ArgumentOutOfRangeException("count",
                    "Audio buffer is full! Check if space is available with DiscordVoiceClient.CanSendVoiceData(int)!");

            audioBuffer.Write(data, offset, count);
        }

        /// <summary>
        /// Gets whether or not the specified number of bytes can be queued
        /// in this <see cref="DiscordVoiceClient"/>.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool CanSendVoiceData(int size)
        {
            return audioBuffer.MaxLength - audioBuffer.Count >= size;
        }

        /// <summary>
        /// Disconnects this <see cref="DiscordVoiceClient"/>.
        /// </summary>
        /// <returns>Returns whether the voice client was disconnected. 
        /// (will return false if already disconnected).</returns>
        public bool Disconnect()
        {
            if (isConnected)
            {
                isConnected = false;
                Client.Gateway.DisconnectFromVoice(Guild);
                OnDisconnected?.Invoke(this, new VoiceClientEventArgs(this));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the speaking state of the <see cref="GuildMember"/>.
        /// Must be set to true for this <see cref="DiscordVoiceClient"/> to send any voice data.
        /// </summary>
        /// <param name="speaking">The speaking state.</param>
        /// <remarks>
        /// Safe to use before this <see cref="DiscordVoiceClient"/> is fully connected,
        /// the last set speaking state will be set upon connecting.
        /// </remarks>
        public void SetSpeaking(bool speaking)
        {
            voiceSocket?.SetSpeaking(speaking);
            isSpeaking = speaking;
        }

        /// <summary>
        /// Clears any queued voice data.
        /// </summary>
        public void ClearVoiceBuffer()
        {
            gotFirstBlock = false;
            audioBuffer.Reset();
        }

        /// <summary>
        /// Signals that this <see cref="DiscordVoiceClient"/> should send all
        /// remaining queued audio without worrying about the speaking state
        /// or pre-buffering.
        /// </summary>
        public void FlushVoiceBuffer()
        {
            isFlushing = true;
        }

        void AudioSendLoop()
        {
            try
            {
                byte[] blockBuffer = new byte[PCM_BLOCK_SIZE];
                while (!isDisposed)
                {
                    if (!isConnected)
                    {
                        Thread.Sleep(2);
                        continue;
                    }

                    if (isSpeaking &&
                        // if already got first block and we have data
                        (!gotFirstBlock || audioBuffer.Count > 0)
                        // if havent got first block but we have over the prebuffer amount
                        && (gotFirstBlock || audioBuffer.Count >= PCM_BLOCK_SIZE * AUDIO_FRAMES_TO_PREBUFFER
                        // if we are flushing and we have data
                        || (isFlushing && audioBuffer.Count > 0)))
                    {
                        if (voiceSocket.CanSendData(blockBuffer.Length))
                        {
                            int read = audioBuffer.Read(blockBuffer, 0, blockBuffer.Length);
                            voiceSocket.SendPCMData(blockBuffer, 0, read);
                            gotFirstBlock = true;
                        }
                    }
                    // Causes major audio artifacts in move to .NET Core
                    // TODO: Figure out why sending opus silence no longer works correctly.
                    //else if (gotFirstBlock && isSpeaking)
                    //{
                    //    if (voiceSocket.CanSendData(3))
                    //    {
                    //        // 5 frames of silence
                    //        byte[] opusSilence = new byte[]
                    //        {
                    //            0xF8,
                    //            0xFF,
                    //            0xFE
                    //        };

                    //        voiceSocket.SendPCMData(opusSilence, 0, opusSilence.Length);
                    //        isFlushing = false;
                    //        Thread.Sleep(1);
                    //    }
                    //}
                    else
                    {
                        isFlushing = false;
                        Thread.Sleep(2);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                log.LogError(ex);

                Disconnect();
                OnFatalError?.Invoke(this, new VoiceClientExceptionEventArgs(this, ex));
            }
        }

        /// <summary>
        /// Disposes and disconnects this <see cref="DiscordVoiceClient"/>.
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                voiceSocket?.Dispose();

                DiscordVoiceClient temp;
                Client.VoiceClients.TryRemove(Guild, out temp);
            }
        }
    }
}
