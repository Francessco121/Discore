using Discore;
using Discore.Voice;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Samples.VoiceSending
{
    class VoiceSession : IDisposable
    {
        public bool IsConnected => connection.IsConnected;
        public bool IsConnecting => connection.IsConnecting;
        public bool IsValid => connection.IsValid;

        readonly DiscordVoiceConnection connection;

        CancellationTokenSource? playCancellationTokenSource;
        TaskCompletionSource? playCompletionSource;
        Thread? playThread;

        public VoiceSession(DiscordVoiceConnection connection)
        {
            this.connection = connection;
            this.connection.OnInvalidated += OnInvalidated;
        }

        /// <exception cref="InvalidOperationException">Thrown if the parent Gateway connection is invalid.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the connection times out or is cancelled.</exception>
        public async Task ConnectOrMove(Snowflake voiceChannelId)
        {
            if (!connection.IsConnected)
                await connection.ConnectAsync(voiceChannelId);
            else
                await connection.UpdateVoiceStateAsync(voiceChannelId);
        }

        public async Task Disconnect()
        {
            await connection.DisconnectAsync();
        }

        public async Task Play(string uri)
        {
            // Cancel the existing task playing audio if it exists
            playCancellationTokenSource?.Cancel();

            // Wait for the existing play task (if necessary) to exit
            if (playCompletionSource != null)
                await playCompletionSource.Task;

            // Start play thread
            playCancellationTokenSource = new CancellationTokenSource();
            playCompletionSource = new TaskCompletionSource();

            playThread = new Thread(PlayThreadMain);
            playThread.Start(uri);

            // Wait for thread to finish
            await playCompletionSource.Task;
        }

        void PlayThreadMain(object? arg)
        {
            string uri = (string)arg!;

            // We'll aggregate stderr data into here so we can report errors
            var errorStringBuilder = new StringBuilder();

            try
            {
                // Start FFmpeg
                using var ffmpeg = new Process();
                ffmpeg.StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Arguments = $"-loglevel warning -i \"{uri}\" -vn -f s16le -ar 48000 -ac 2 pipe:1"
                };

                ffmpeg.ErrorDataReceived += (sender, e) =>
                {
                    errorStringBuilder.AppendLine(e.Data);
                };

                if (ffmpeg.Start())
                {
                    ffmpeg.BeginErrorReadLine();

                    // Notify Discord that we are starting
                    connection.SetSpeakingAsync(true).Wait();

                    // Create a buffer to move data from FFmpeg to the voice connection
                    byte[] transferBuffer = new byte[DiscordVoiceConnection.PCM_BLOCK_SIZE];

                    // Keep moving data from FFmpeg until the stream is complete, the connection is invalidated,
                    // or the play task is cancelled
                    while (!playCancellationTokenSource!.IsCancellationRequested &&
                        connection.IsValid &&
                        !ffmpeg.StandardOutput.EndOfStream)
                    {
                        // Ensure the connection's buffer has room for a full block of audio
                        if (connection.CanSendVoiceData(transferBuffer.Length))
                        {
                            // Read data from FFmpeg
                            int read = ffmpeg.StandardOutput.BaseStream.Read(transferBuffer, 0, transferBuffer.Length);

                            // Send the data over the voice connection
                            connection.SendVoiceData(transferBuffer, 0, read);
                        }
                        else
                        {
                            // Otherwise wait a short amount of time to avoid burning CPU cycles
                            Thread.Sleep(1);
                        }
                    }

                    // Let everything get written to the socket before exiting
                    while (!playCancellationTokenSource.IsCancellationRequested &&
                        connection.IsValid &&
                        connection.BytesToSend > 0)
                    {
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    Console.WriteLine("Failed to start FFmpeg!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception in voice session: {ex}");
            }
            finally
            {
                try
                {
                    // Notify Discord that we have stopped sending audio
                    connection.SetSpeakingAsync(false).Wait();
                }
                catch { /* It's ok if this errors */ }

                // Allow play to be called again
                playCompletionSource!.SetResult();

                // Log any errors
                string stderr = errorStringBuilder.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(stderr))
                    Console.WriteLine($"FFmpeg error: {stderr}");
            }
        }

        public bool Stop()
        {
            // Cancel the existing task playing audio if it exists
            if (playCancellationTokenSource != null && !playCancellationTokenSource.IsCancellationRequested)
            {
                playCancellationTokenSource.Cancel();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Dispose()
        {
            connection.OnInvalidated -= OnInvalidated;
            connection.Dispose();
            playCancellationTokenSource?.Cancel();
            playCancellationTokenSource?.Dispose();
        }

        void OnInvalidated(object? sender, VoiceConnectionInvalidatedEventArgs e)
        {
            // Connection is dead, make sure our play thread exits
            playCancellationTokenSource?.Cancel();
        }
    }
}
