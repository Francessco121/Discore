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
        /// Called when this <see cref="DiscordVoiceClient"/> finishes connecting to Discord.
        /// </summary>
        public event EventHandler<VoiceClientEventArgs> OnConnected;
        /// <summary>
        /// Called when this <see cref="DiscordVoiceClient"/> is disposed and is no longer valid.
        /// </summary>
        public event EventHandler<VoiceClientEventArgs> OnDisposed;

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
        public bool IsValid { get { return !disposed; } }
        /// <summary>
        /// Gets whether or not this <see cref="DiscordVoiceClient"/> is connected to a <see cref="DiscordGuild"/>.
        /// </summary>
        public bool IsConnected { get { return isConnected; } }

        // TODO: Make prebuffer configuration available publically
        const int PCM_BLOCK_SIZE = 3840;
        const int AUDIO_FRAMES_TO_PREBUFFER = 64;
        const int AUDIO_FRAMES_TO_BUFFER = 96;
        const int AUDIO_BUFFER_SIZE = PCM_BLOCK_SIZE * AUDIO_FRAMES_TO_BUFFER;

        CircularBuffer audioBuffer;
        VoiceSocket voiceSocket;
        bool disposed;
        bool isSpeaking;
        bool isFlushing;
        bool isConnected;
        bool gotFirstBlock;
        Thread audioSendThread;

        internal DiscordVoiceClient(DiscordClient client, DiscordGuild guild)
        {
            Client = client;
            Guild = guild;
            
            audioBuffer = new CircularBuffer(AUDIO_BUFFER_SIZE);

            audioSendThread = new Thread(AudioSendLoop);
            audioSendThread.Name = "Audio Send Thread";
            audioSendThread.IsBackground = true;
            audioSendThread.Start();
        }

        internal void SetSocket(VoiceSocket socket)
        {
            voiceSocket = socket;

            // Set speaking could have been called before the
            // socket arrived, so set it based on the last call.
            voiceSocket.SetSpeaking(isSpeaking);

            isConnected = true;

            OnConnected?.Invoke(this, new VoiceClientEventArgs(this));
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
        public void Disconnect()
        {
            if (isConnected)
            {
                isConnected = false;
                Client.Gateway.DisconnectFromVoice(Guild);
            }

            Dispose();
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
                while (!disposed)
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
                        int read = audioBuffer.Read(blockBuffer, 0, blockBuffer.Length);
                        voiceSocket.SendPCMData(blockBuffer, 0, read);
                        gotFirstBlock = true;
                    }
                    else if (gotFirstBlock && isSpeaking)
                    {
                        // 5 frames of silence
                        byte[] opusSilence = new byte[]
                        {
                            0xF8,
                            0xFF,
                            0xFE
                        };

                        voiceSocket.SendPCMData(opusSilence, 0, opusSilence.Length);
                        isFlushing = false;
                        Thread.Sleep(1);
                    }
                    else
                    {
                        isFlushing = false;
                        Thread.Sleep(2);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Client.EnqueueError(e);
            }
            finally
            {
                Dispose();
            }
        }

        /// <summary>
        /// Disposes and disconnects this <see cref="DiscordVoiceClient"/>.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                isConnected = false;
                isSpeaking = false;

                voiceSocket?.Dispose();

                OnDisposed?.Invoke(this, new VoiceClientEventArgs(this));

                DiscordVoiceClient temp;
                Client.VoiceClients.TryRemove(Guild, out temp);
            }
        }
    }
}
