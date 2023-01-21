using Discore.WebSocket;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Voice
{
    /// <summary>
    /// A connection to voice for a single guild.
    /// </summary>
    /// <remarks>
    /// Only one voice connection can exist at a time per guild.
    /// Voice connections can only be connected to one voice channel at a time.
    /// </remarks>
    public sealed partial class DiscordVoiceConnection : IDisposable
    {
        /// <summary>
        /// The byte size of a single PCM audio block.
        /// </summary>
        public const int PCM_BLOCK_SIZE = 3840;

        /// <summary>
        /// Called when the voice connection first connects or reconnects.
        /// </summary>
        public event EventHandler<VoiceConnectionEventArgs>? OnConnected;
        /// <summary>
        /// Called when this voice connection is no longer useable. (eg. disconnected, error, failure to connect).
        /// </summary>
        public event EventHandler<VoiceConnectionInvalidatedEventArgs>? OnInvalidated;
        /// <summary>
        /// Called when another user in the voice channel this connection is connected to changes their speaking state.
        /// </summary>
        public event EventHandler<MemberSpeakingEventArgs>? OnMemberSpeaking;

        /// <summary>
        /// Gets the ID of the guild this voice connection is in.
        /// </summary>
        public Snowflake GuildId => guildId;

        /// <summary>
        /// Gets the ID of the current voice channel this connection is in.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if used before the connection has been initiated.</exception>
        public Snowflake VoiceChannelId
        {
            get
            {
                if (voiceState == null || !voiceState.ChannelId.HasValue)
                    throw new InvalidOperationException("Cannot retrieve voice channel ID before connecting!");

                return voiceState.ChannelId.Value;
            }
        }
        /// <summary>
        /// Gets whether this connection is connected.
        /// </summary>
        public bool IsConnected => isConnected;
        /// <summary>
        /// Gets whether this connection is currently performing its handshake.
        /// </summary>
        public bool IsConnecting => isConnecting;
        /// <summary>
        /// Gets whether this connection is available to use.
        /// </summary>
        public bool IsValid => isValid;
        /// <summary>
        /// Gets whether this connection is currently set to "speaking". 
        /// </summary>
        /// <seealso cref="SpeakingFlags"/>
        public bool IsSpeaking => speakingFlags != SpeakingFlag.Off;
        /// <summary>
        /// Gets the speaking state of this connection. 
        /// </summary>
        public SpeakingFlag SpeakingFlags => speakingFlags;
        /// <summary>
        /// Gets the number of unsent voice data bytes.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if used before being fully connected.</exception>
        public int BytesToSend
        {
            get
            {
                if (!isConnected || udpSocket == null)
                    throw new InvalidOperationException("Cannot retrieve bytes to send while not fully connected.");

                return udpSocket.BytesToSend;
            }
        }
        /// <summary>
        /// Gets or sets whether the sending of voice data is paused.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if used before being fully connected.</exception>
        public bool IsPaused
        {
            get
            {
                if (!isConnected || udpSocket == null)
                    throw new InvalidOperationException("Cannot retrieve paused state while not fully connected.");

                return udpSocket.IsPaused;
            }
            set
            {
                if (!isConnected || udpSocket == null)
                    throw new InvalidOperationException("Cannot set paused state while not fully connected.");

                udpSocket.IsPaused = value;
            }
        }

        readonly Snowflake guildId;
        readonly Snowflake userId;
        readonly IGatewayVoiceBridge bridge;
        readonly DefaultGatewayVoiceBridge? defaultBridge;

        DiscordVoiceState? voiceState;
        readonly DiscoreLogger log;

        bool isDisposed;
        bool isValid;
        bool isConnected;
        bool isConnecting;

        CancellationTokenSource? connectingCancellationSource;

        SpeakingFlag speakingFlags;

        /// <summary>
        /// Creates a new voice connection for a guild managed by the given shard.
        /// </summary>
        /// <param name="shard">The shard that manages the guild.</param>
        /// <param name="guildId">The guild to connect to.</param>
        /// <exception cref="ArgumentException">Thrown if the given shard is not running.</exception>
        public DiscordVoiceConnection(Shard shard, Snowflake guildId)
        {
            // We need to guarantee that the shard user ID is available,
            // so double-check that the shard is running.
            if (!shard.IsRunning)
                throw new ArgumentException("Voice connection cannot be created while the parent shard is not running!");

            defaultBridge = new DefaultGatewayVoiceBridge(shard.Gateway);
            bridge = defaultBridge;
            userId = shard.UserId!.Value;

            this.guildId = guildId;

            log = new DiscoreLogger($"VoiceConnection:{guildId}");

            isValid = true;
        }

        /// <summary>
        /// Creates a new voice connection for a guild.
        /// </summary>
        /// <param name="bridge">A bridge between the Gateway connection that serves the guild and this voice connection.</param>
        /// <param name="userId">The user ID of the application/bot.</param>
        /// <param name="guildId">The guild to connect to.</param>
        /// <remarks>
        /// This constructor should only be used if you are implementing <see cref="IGatewayVoiceBridge"/> yourself to
        /// create a bridge between your application's voice connections and the Gateway connection. If the voice
        /// connections live in the same process as the Gateway connection, prefer the simpler constructor that
        /// takes a shard instance.
        /// </remarks>
        public DiscordVoiceConnection(IGatewayVoiceBridge bridge, Snowflake userId, Snowflake guildId)
        {
            this.bridge = bridge;
            this.userId = userId;
            this.guildId = guildId;

            log = new DiscoreLogger($"VoiceConnection:{guildId}");

            isValid = true;
        }

        /// <summary>
        /// Initiates this voice connection.
        /// <para>
        /// Note: An <see cref="OperationCanceledException"/> will be thrown if the Gateway 
        /// connection is closed while initiating.
        /// </para>
        /// </summary>
        /// <param name="startMute">Whether the current bot should connect self-muted.</param>
        /// <param name="startDeaf">Whether the current bot should connect self-deafened.</param>
        /// <param name="connectionTimeout">The maximum amount of time to wait for connection to complete before timing out.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if connect is called more than once or if the parent Gateway connection is not valid.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the give cancellation token is cancelled, the Gateway connection is closed while initiating the voice connection,
        /// or if the <paramref name="connectionTimeout"/> is passed.
        /// </exception>
        public async Task ConnectAsync(Snowflake voiceChannelId,
            bool startMute = false, bool startDeaf = false,
            TimeSpan? connectionTimeout = null,
            CancellationToken? cancellationToken = null)
        {
            if (isValid)
            {
                if (!isConnecting && !IsConnected)
                {
                    // Initiate the connection
                    log.LogInfo($"Connecting to channel {voiceChannelId}...");

                    isConnecting = true;
                    connectingCancellationSource = new CancellationTokenSource();

                    SubscribeEvents();

                    await bridge.UpdateVoiceStateAsync(guildId, voiceChannelId,
                        startMute, startDeaf, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);

                    connectionTimeout ??= TimeSpan.FromSeconds(10);

                    await ConnectionTimeout(connectionTimeout.Value, connectingCancellationSource.Token).ConfigureAwait(false);

                    if (isConnected)
                    {
                        log.LogInfo("Connected!");
                    }
                    else
                    {
                        log.LogWarning("Connection timed out.");
                        throw new OperationCanceledException("Voice connection timed out.");
                    }
                }
                else
                    throw new InvalidOperationException("Voice connection is already connecting or is currently connected.");
            }
        }

        async Task ConnectionTimeout(TimeSpan timeout, CancellationToken cancellationToken)
        {
            try
            {
                // Wait
                await Task.Delay(timeout, cancellationToken).ConfigureAwait(false);

                // If still not connected, timeout and disconnect.
                if (isConnecting)
                {
                    await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "Timed out while completing handshake",
                        VoiceConnectionInvalidationReason.TimedOut, "Timed out while completing handshake.").ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // connectingCancellationSource was cancelled because we connected successfully.
            }
        }

        /// <summary>
        /// Closes this voice connection.
        /// <para>Note: The connection will still be closed if the passed cancellation token is cancelled.</para>
        /// </summary>
        /// <param name="cancellationToken">A token which will force close the connection when cancelled.</param>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this voice connection is not connected.</exception>
        /// <exception cref="OperationCanceledException"></exception>
        public async Task DisconnectAsync(CancellationToken? cancellationToken = null)
        {
            if (!isConnected)
                throw new InvalidOperationException("The voice connection is not connected!");

            await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "Closing normally...", 
                VoiceConnectionInvalidationReason.Normal, null, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets whether the specified number of bytes can currently 
        /// be sent to this voice connection.
        /// Will return false if not yet connected or invalid.
        /// </summary>
        public bool CanSendVoiceData(int size)
        {
            return isValid && isConnected && !isConnecting
                && (udpSocket != null && udpSocket.CanSendData(size));
        }

        /// <summary>
        /// Queues the specified PCM bytes to be sent over this voice connection. 
        /// <para>
        /// The size of the data should be equal to or less than <see cref="PCM_BLOCK_SIZE"/>.
        /// Should be used along-side <see cref="CanSendVoiceData(int)"/> to avoid overflowing the buffer.
        /// </para>
        /// <para>
        /// This call will not block and users do not need to worry about calling this at a certain rate.
        /// </para>
        /// <para>
        /// Does nothing if this connection is invalid.
        /// </para>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified number of bytes will exceed the buffer size.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the voice connection is not fully connected yet. Checking <see cref="CanSendVoiceData(int)"/>
        /// first will avoid this.
        /// </exception>
        public void SendVoiceData(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (isValid)
            {
                if (udpSocket == null || !isConnected || isConnecting)
                    throw new InvalidOperationException("Cannot send voice data before being connected!");

                udpSocket.SendData(buffer, offset, count);
            }
        }

        /// <summary>
        /// Sets the speaking state of this connection.
        /// <para/>
        /// Does nothing if this connection is invalid.
        /// </summary>
        /// <exception cref="DiscordWebSocketException">Thrown if the state fails to set because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the voice connection is not fully connected yet.</exception>
        public Task SetSpeakingAsync(bool speaking)
        {
            return SetSpeakingAsync(speaking ? SpeakingFlag.Microphone : SpeakingFlag.Off);
        }

        /// <summary>
        /// Sets the speaking state of this connection.
        /// <para/>
        /// Does nothing if this connection is invalid.
        /// </summary>
        /// <exception cref="DiscordWebSocketException">Thrown if the state fails to set because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the voice connection is not fully connected yet.</exception>
        public Task SetSpeakingAsync(SpeakingFlag flags)
        {
            if (isValid)
            {
                if (webSocket == null || udpSocket == null || !isConnected || isConnecting)
                    throw new InvalidOperationException("Cannot set speaking state before being connected!");

                speakingFlags = flags;

                return webSocket!.SendSpeakingPayload(flags, udpSocket!.Ssrc);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Clears all queued voice data.
        /// <para/>
        /// Does nothing if this connection is invalid.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the voice connection is not fully connected yet.</exception>
        public void ClearVoiceBuffer()
        {
            if (isValid)
            {
                if (udpSocket == null || !isConnected || isConnecting)
                    throw new InvalidOperationException("Cannot clear voice data before being connected!");

                udpSocket.ClearVoiceBuffer();
            }
        }

        /// <summary>
        /// Moves channels and/or changes the applications self muted/deafened states.
        /// </summary>
        /// <param name="voiceChannelId">
        /// The ID of the voice channel to move to. Specify the channel the application is already in to stay there.
        /// </param>
        /// <param name="isMute">Whether the application is self-mute.</param>
        /// <param name="isDeaf">Whether the application is self-deafened.</param>
        /// <param name="cancellationToken">A token used to cancel the request.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="voiceChannelId"/> is an empty snowflake.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the parent Gateway connection is no longer valid.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the cancellation token is cancelled. </exception>
        /// <remarks>
        /// Do not pass an empty snowflake for the voice channel ID to disconnect. Instead, call <see cref="DisconnectAsync"/>.
        /// </remarks>
        public async Task UpdateVoiceStateAsync(Snowflake voiceChannelId, bool isMute, bool isDeaf,
            CancellationToken? cancellationToken = null)
        {
            if (voiceChannelId == Snowflake.None)
                throw new ArgumentException("The voice channel ID must be a valid channel ID.", nameof(voiceChannelId));

            if (isValid)
            {
                await bridge.UpdateVoiceStateAsync(guildId, voiceChannelId,
                    isMute, isDeaf, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Releases all resources used by this voice connection.
        /// <para>Note: this will not invalidate the voice connection.</para>
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                defaultBridge?.Dispose();

                connectingCancellationSource?.Dispose();

                webSocket?.Dispose();
                udpSocket?.Dispose();
            }
        }
    }
}
