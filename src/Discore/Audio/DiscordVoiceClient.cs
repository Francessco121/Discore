using System;
using System.Threading;

namespace Discore.Audio
{
    public class DiscordVoiceClient : IDisposable
    {
        public const int PCM_BLOCK_SIZE = 3840;

        /// <summary>
        /// The guild this client is in.
        /// </summary>
        public DiscordGuild Guild { get; }
        /// <summary>
        /// The guild member this client is communicating through.
        /// </summary>
        public DiscordGuildMember GuildMember { get { return voiceSocket.Member; } }

        public event EventHandler<VoiceClientEventArgs> OnConnected;
        public event EventHandler<VoiceClientEventArgs> OnDisposed;

        public readonly DiscordClient Client;

        public int BytesToSend { get { return audioBuffer.Count; } }

        public bool IsValid { get { return !disposed; } }
        public bool IsConnected { get { return isConnected; } }

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

        public void SendVoiceData(byte[] data, int offset, int count)
        {
            if (audioBuffer.MaxLength - audioBuffer.Count < count)
                throw new ArgumentOutOfRangeException("count",
                    "Audio buffer is full! Check if space is available with DiscordVoiceClient.CanSendVoiceData(int)!");

            audioBuffer.Write(data, offset, count);
        }

        public bool CanSendVoiceData(int size)
        {
            return audioBuffer.MaxLength - audioBuffer.Count >= size;
        }

        public void Disconnect()
        {
            if (isConnected)
            {
                isConnected = false;
                Client.Gateway.DisconnectFromVoice(Guild);
            }

            Dispose();
        }

        public void SetSpeaking(bool speaking, bool resetAudioBuffer = true)
        {
            voiceSocket?.SetSpeaking(speaking);
            isSpeaking = speaking;

            if (resetAudioBuffer)
            {
                // Reset for next time.
                gotFirstBlock = false;
                audioBuffer.Reset();
            }
        }

        public void ClearVoiceBuffer()
        {
            audioBuffer.Reset();
        }

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
