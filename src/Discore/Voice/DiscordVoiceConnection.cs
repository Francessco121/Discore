using Discore.Voice.Net;
using Discore.WebSocket;
using Discore.WebSocket.Net;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Voice
{
    public class VoiceConnectionEventArgs : EventArgs
    {
        public Shard Shard { get; }
        public DiscordVoiceConnection Connection { get; }

        internal VoiceConnectionEventArgs(Shard shard, DiscordVoiceConnection connection)
        {
            Shard = shard;
            Connection = connection;
        }
    }

    public class VoiceConnectionErrorEventArgs : VoiceConnectionEventArgs
    {
        public Exception Exception { get; }

        internal VoiceConnectionErrorEventArgs(Shard shard, DiscordVoiceConnection connection, Exception exception)
            : base(shard, connection)
        {
            Exception = exception;
        }
    }

    public sealed class DiscordVoiceConnection : IDisposable
    {
        /// <summary>
        /// The byte size of a single PCM audio block.
        /// </summary>
        public const int PCM_BLOCK_SIZE = 3840;

        /// <summary>
        /// Called when the voice connection first connects or reconnects.
        /// </summary>
        public event EventHandler<VoiceConnectionEventArgs> OnConnected;
        /// <summary>
        /// Called when the voice connection is disconnected.
        /// </summary>
        public event EventHandler<VoiceConnectionEventArgs> OnDisconnected;
        /// <summary>
        /// Called when the voice connection unexpectedly closes or encounters an error while connecting.
        /// </summary>
        public event EventHandler<VoiceConnectionErrorEventArgs> OnError;
        /// <summary>
        /// Called when this voice connection is no longer useable. (eg. disconnected, error, failure to connect).
        /// </summary>
        public event EventHandler<VoiceConnectionEventArgs> OnInvalidated;

        /// <summary>
        /// Gets the shard this connection is managed by.
        /// </summary>
        public Shard Shard { get; }
        /// <summary>
        /// Gets the guild this voice connection is in.
        /// </summary>
        public DiscordGuild Guild => guildCache.Value;
        /// <summary>
        /// Gets the member this connection is communicating through.
        /// </summary>
        public DiscordGuildMember Member => memberCache.Value;

        /// <summary>
        /// Gets the current voice channel this connection is in.
        /// </summary>
        public DiscordGuildVoiceChannel VoiceChannel
        {
            // Voice state will not be immediately available,
            // so return the initial voice channel while we are still connecting.
            get
            {
                if (voiceState != null)
                {
                    DiscordGuildVoiceChannel voiceChannel = voiceState.Channel;
                    if (voiceChannel != null)
                        return voiceChannel;
                }

                return initialVoiceChannel;
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
        /// Gets or sets the speaking state of this connection.
        /// </summary>
        public bool IsSpeaking => isSpeaking;
        /// <summary>
        /// Gets the number of unsent voice data bytes.
        /// </summary>
        public int BytesToSend => udpSocket.BytesToSend;
        /// <summary>
        /// Gets or sets whether the sending of voice data is paused.
        /// </summary>
        public bool IsPaused
        {
            get => udpSocket.IsPaused;
            set => udpSocket.IsPaused = value;
        }

        DiscoreGuildCache guildCache;
        DiscoreMemberCache memberCache;

        Gateway gateway;

        VoiceWebSocket webSocket;
        VoiceUdpSocket udpSocket;

        DiscordVoiceState voiceState;
        DiscoreLogger log;
        DiscordGuildVoiceChannel initialVoiceChannel;

        string token;
        string endPoint;
        bool isDisposed;
        bool isValid;
        bool isConnected;
        bool isConnecting;

        CancellationTokenSource connectingCancellationSource;

        bool isSpeaking;

        internal DiscordVoiceConnection(Shard shard, Gateway gateway, DiscoreGuildCache guildCache, DiscoreMemberCache memberCache,
            DiscordGuildVoiceChannel initialVoiceChannel)
        {
            Shard = shard;

            this.gateway = gateway;
            this.guildCache = guildCache;
            this.memberCache = memberCache;
            this.initialVoiceChannel = initialVoiceChannel;

            log = new DiscoreLogger($"VoiceConnection:{guildCache.Value.Name}");

            isValid = true;
            isSpeaking = true;
        }

        private async void UdpSocket_OnClosedPrematurely(object sender, EventArgs e)
        {
            await CloseAndInvalidate(DiscordClientWebSocket.INTERNAL_CLIENT_ERROR, "An internal client error occured.")
                .ConfigureAwait(false);

            DiscoreException ex = new DiscoreException("The UDP connection closed unexpectedly.");
            OnError?.Invoke(this, new VoiceConnectionErrorEventArgs(Shard, this, ex));
        }

        private async void WebSocket_OnUnexpectedClose(object sender, EventArgs e)
        {
            await CloseAndInvalidate(DiscordClientWebSocket.INTERNAL_CLIENT_ERROR, "An internal client error occured.")
                .ConfigureAwait(false);

            DiscoreException ex = new DiscoreException("The WebSocket connection closed unexpectedly.");
            OnError?.Invoke(this, new VoiceConnectionErrorEventArgs(Shard, this, ex));
        }

        private async void WebSocket_OnTimedOut(object sender, EventArgs e)
        {
            await CloseAndInvalidate(DiscordClientWebSocket.INTERNAL_CLIENT_ERROR, "Connection timed out.")
                .ConfigureAwait(false);

            DiscoreException ex = new DiscoreException("The WebSocket connection timed out.");
            OnError?.Invoke(this, new VoiceConnectionErrorEventArgs(Shard, this, ex));
        }

        /// <summary>
        /// Ensures that: the WebSocket is closed, the UDP socket is closed, the user has left the voice channel,
        /// and the connection is invalidated.
        /// </summary>
        /// <exception cref="OperationCanceledException"></exception>
        async Task CloseAndInvalidate(WebSocketCloseStatus webSocketCloseCode, string webSocketCloseDescription,
            CancellationToken? cancellationToken = null)
        {
            await EnsureWebSocketIsClosed(webSocketCloseCode, webSocketCloseDescription, cancellationToken)
                .ConfigureAwait(false);

            EnsureUdpSocketIsClosed();

            await EnsureUserLeftVoiceChannel(cancellationToken ?? CancellationToken.None)
                .ConfigureAwait(false);

            Invalidate();
        }

        /// <summary>
        /// Initiates this voice connection.
        /// </summary>
        /// <param name="startMute">Whether the authenticated user should connect self-muted.</param>
        /// <param name="startDeaf">Whether the authenticated user should connect self-deafened.</param>
        /// <exception cref="InvalidOperationException">Thrown if connect is called more than once.</exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the Gateway connection is closed while initiating the voice connection.
        /// </exception>
        [Obsolete("Please use the asynchronous counterpart ConnectAsync(bool, bool) instead.")]
        public void Connect(bool startMute = false, bool startDeaf = false)
        {
            ConnectAsync().Wait();
        }

        /// <summary>
        /// Initiates this voice connection.
        /// <para>
        /// Note: An <see cref="OperationCanceledException"/> will be thrown if the Gateway 
        /// connection is closed while initiating.
        /// </para>
        /// </summary>
        /// <param name="startMute">Whether the authenticated user should connect self-muted.</param>
        /// <param name="startDeaf">Whether the authenticated user should connect self-deafened.</param>
        /// <exception cref="InvalidOperationException">Thrown if connect is called more than once.</exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the give cancellation token is cancelled or the Gateway connection is closed while initiating the voice connection.
        /// </exception>
        public async Task ConnectAsync(bool startMute = false, bool startDeaf = false, CancellationToken? cancellationToken = null)
        {
            if (isValid)
            {
                if (!isConnecting && !IsConnected)
                {
                    isConnecting = true;
                    await gateway.SendVoiceStateUpdatePayload(initialVoiceChannel.GuildId, initialVoiceChannel.Id, 
                        startMute, startDeaf, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);

                    connectingCancellationSource = new CancellationTokenSource();

                    await ConnectionTimeout().ConfigureAwait(false);
                }
                else
                    throw new InvalidOperationException("Voice connection is already connecting or is currently connected.");
            }
        }

        async Task ConnectionTimeout()
        {
            try
            {
                // Wait 10s
                await Task.Delay(10000, connectingCancellationSource.Token).ConfigureAwait(false);

                // If still not connected, timeout and disconnect.
                if (isConnecting)
                {
                    await CloseAndInvalidate(DiscordClientWebSocket.INTERNAL_CLIENT_ERROR, "Timed out while completing handshake")
                        .ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // connectingCancellationSource was cancelled because we connected successfully.
            }
        }

        /// <summary>
        /// Closes this voice connection.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this voice connection is not connected.</exception>
        [Obsolete("Please use the asynchronous counterpart DisconnectAsync(CancellationToken) instead.")]
        public bool Disconnect()
        {
            return DisconnectAsync(CancellationToken.None).Result;
        }

        /// <summary>
        /// Closes this voice connection.
        /// <para>Note: The connection will still be closed if the passed cancellation token is cancelled.</para>
        /// </summary>
        /// <param name="cancellationToken">A token which will force close the connection when cancelled.</param>
        /// <returns>Returns whether the operation was successful.</returns>
        /// <exception cref="InvalidOperationException">Thrown if this voice connection is not connected.</exception>
        /// <exception cref="OperationCanceledException"></exception>
        public async Task<bool> DisconnectAsync(CancellationToken cancellationToken)
        {
            if (isValid)
            {
                if (!isConnected)
                    throw new InvalidOperationException("The voice connection is not connected!");

                try
                {
                    await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "Closing normally...", cancellationToken)
                        .ConfigureAwait(false);

                    return true;
                }
                finally
                {
                    OnDisconnected?.Invoke(this, new VoiceConnectionEventArgs(Shard, this));
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Gets whether the specified number of bytes can currently 
        /// be sent to this voice connection.
        /// Will return false if not yet connected or invalid.
        /// </summary>
        public bool CanSendVoiceData(int size)
        {
            return isValid && IsConnected && udpSocket.CanSendData(size);
        }

        /// <summary>
        /// Sends the specified PCM bytes to this voice connection.
        /// <para>
        /// The size of the data sent should be equal to or less than <see cref="PCM_BLOCK_SIZE"/>.
        /// </para>
        /// <para>
        /// Should be used along-side <see cref="CanSendVoiceData(int)"/> to
        /// avoid overflowing the buffer.
        /// </para>
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified number of bytes will exceed the buffer size.
        /// </exception>
        public void SendVoiceData(byte[] buffer, int offset, int count)
        {
            if (isValid)
            {
                udpSocket.SendData(buffer, offset, count);
            }
        }

        /// <summary>
        /// Sets the speaking state of this connection.
        /// </summary>
        [Obsolete("Please use the asynchronous counterpart SetSpeakingAsync(bool) instead.")]
        public void SetSpeaking(bool speaking)
        {
            SetSpeakingAsync(speaking).Wait();
        }

        /// <summary>
        /// Sets the speaking state of this connection.
        /// </summary>
        /// <exception cref="DiscordWebSocketException">Thrown if the state fails to set because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SetSpeakingAsync(bool speaking)
        {
            if (isValid)
            {
                isSpeaking = speaking;

                if (IsConnected)
                    return webSocket.SendSpeakingPayload(speaking);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Clears all queued voice data.
        /// </summary>
        public void ClearVoiceBuffer()
        {
            if (isValid)
            {
                udpSocket.ClearVoiceBuffer();
            }
        }

        internal async Task OnVoiceStateUpdated(DiscordVoiceState voiceState)
        {
            if (isValid)
            {
                this.voiceState = voiceState;

                if (!isConnected && !isConnecting && token != null && endPoint != null)
                    // Either the token or session id can be received first,
                    // so we must check if we are ready to start in both cases.
                    await ConnectWebSocket().ConfigureAwait(false);
            }
        }

        internal async Task OnVoiceServerUpdated(string token, string endPoint)
        {
            if (isValid)
            {
                this.token = token;
                this.endPoint = endPoint.Split(':')[0]; // TODO: whats the other pieces?

                if (voiceState != null)
                {
                    // Server updates can be sent twice, the second time
                    // is when the voice server changes, so we need to reconnect.
                    if (isConnected)
                    {
                        await EnsureWebSocketIsClosed(WebSocketCloseStatus.NormalClosure, "Reconnecting...").ConfigureAwait(false);
                        EnsureUdpSocketIsClosed();
                    }

                    // Either the token or session id can be received first,
                    // so we must check if we are ready to start in both cases.
                    await ConnectWebSocket().ConfigureAwait(false);
                }
            }
        }

        private async void WebSocket_OnReady(object sender, VoiceReadyEventArgs e)
        {
            if (!isValid || !isConnected)
                return;

            try
            {
                // Connect the UDP socket
                await ConnectUdpSocket(e.Port).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogError($"[OnReady] Failed to connect UDP socket: {ex}");

                await CloseAndInvalidate(DiscordClientWebSocket.INTERNAL_CLIENT_ERROR, "An internal client error occured.")
                    .ConfigureAwait(false);

                OnError?.Invoke(this, new VoiceConnectionErrorEventArgs(Shard, this, ex));

                return;
            }

            // Give the SSRC to the UDP socket
            udpSocket.SetSsrc(e.Ssrc);

            try
            {
                // Start IP discovery
                await udpSocket.StartIPDiscoveryAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogError($"[OnReady] Failed start IP discovery: {ex}");

                await CloseAndInvalidate(DiscordClientWebSocket.INTERNAL_CLIENT_ERROR, "An internal client error occured.")
                    .ConfigureAwait(false);

                OnError?.Invoke(this, new VoiceConnectionErrorEventArgs(Shard, this, ex));

                return;
            }
        }

        private async void UdpSocket_OnIPDiscovered(object sender, IPDiscoveryEventArgs e)
        {
            if (!isValid || !isConnected)
                return;

            log.LogVerbose($"[IPDiscovery] Discovered IP: {e.IP}:{e.Port}");

            try
            {
                // Select protocol
                await webSocket.SendSelectProtocolPayload(e.IP, e.Port, "xsalsa20_poly1305").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogError($"[OnIPDiscovered] Failed to select protocol: {ex}");

                await CloseAndInvalidate(DiscordClientWebSocket.INTERNAL_CLIENT_ERROR, "An internal client error occured.")
                    .ConfigureAwait(false);

                OnError?.Invoke(this, new VoiceConnectionErrorEventArgs(Shard, this, ex));

                return;
            }
        }

        private void WebSocket_OnSessionDescription(object sender, VoiceSessionDescriptionEventArgs e)
        {
            if (!isValid || !isConnected)
                return;
            
            // Give the UDP socket the secret key to allow sending data.
            udpSocket.Start(e.SecretKey);
        }

        /// <exception cref="ArgumentException">Thrown if the socket host resolved into zero addresses.</exception>
        /// <exception cref="SocketException">Thrown if the host fails to resolve or the socket fails to connect.</exception>
        async Task ConnectUdpSocket(int port)
        {
            // Create UDP Socket
            udpSocket = new VoiceUdpSocket($"VoiceUDPSocket:{guildCache.Value.Name}");
            udpSocket.OnIPDiscovered += UdpSocket_OnIPDiscovered;
            udpSocket.OnClosedPrematurely += UdpSocket_OnClosedPrematurely;

            // Connect UDP socket
            await udpSocket.ConnectAsync(endPoint, port).ConfigureAwait(false);
        }

        async Task ConnectWebSocket()
        {
            // Create WebSocket
            webSocket = new VoiceWebSocket($"VoiceWebSocket:{guildCache.Value.Name}");
            webSocket.OnReady += WebSocket_OnReady;
            webSocket.OnSessionDescription += WebSocket_OnSessionDescription;
            webSocket.OnUnexpectedClose += WebSocket_OnUnexpectedClose;
            webSocket.OnTimedOut += WebSocket_OnTimedOut;

            // Build WebSocket URI
            Uri uri = new Uri($"wss://{endPoint}");

            log.LogVerbose($"Connecting WebSocket to {uri}...");

            try
            {
                // Connect WebSocket
                await webSocket.ConnectAsync(uri, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to connect to {uri}: {ex}");

                await CloseAndInvalidate(DiscordClientWebSocket.INTERNAL_CLIENT_ERROR, "An internal client error occured.")
                    .ConfigureAwait(false);

                OnError?.Invoke(this, new VoiceConnectionErrorEventArgs(Shard, this, ex));

                return;
            }

            log.LogVerbose("Connected WebSocket.");

            try
            {
                // Send IDENTIFY payload
                await webSocket.SendIdentifyPayload(guildCache.DictionaryId, memberCache.DictionaryId,
                    memberCache.VoiceState.SessionId, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogError($"[ConnectSocket] Failed to send identify payload: {ex}");

                await CloseAndInvalidate(DiscordClientWebSocket.INTERNAL_CLIENT_ERROR, "An internal client error occured.")
                    .ConfigureAwait(false);

                OnError?.Invoke(this, new VoiceConnectionErrorEventArgs(Shard, this, ex));

                return;
            }

            // We are finished connecting
            isConnected = true;
            isConnecting = false;
            connectingCancellationSource.Cancel();

            try
            {
                // Set initial speaking state
                await SetSpeakingAsync(isSpeaking).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogError($"[ConnectSocket] Failed to set initial speaking state: {ex}");

                await CloseAndInvalidate(DiscordClientWebSocket.INTERNAL_CLIENT_ERROR, "An internal client error occured.")
                    .ConfigureAwait(false);

                OnError?.Invoke(this, new VoiceConnectionErrorEventArgs(Shard, this, ex));

                return;
            }

            OnConnected?.Invoke(this, new VoiceConnectionEventArgs(Shard, this));
        }

        /// <exception cref="OperationCanceledException"></exception>
        async Task EnsureWebSocketIsClosed(WebSocketCloseStatus webSocketCloseStatus, string webSocketCloseDescription,
            CancellationToken? cancellationToken = null)
        {
            isConnected = false;

            if (webSocket != null)
            {
                webSocket.OnReady -= WebSocket_OnReady;
                webSocket.OnSessionDescription -= WebSocket_OnSessionDescription;
                webSocket.OnUnexpectedClose -= WebSocket_OnUnexpectedClose;
                webSocket.OnTimedOut -= WebSocket_OnTimedOut;

                if (webSocket.CanBeDisconnected)
                {
                    try
                    {
                        await webSocket.DisconnectAsync(webSocketCloseStatus, webSocketCloseDescription,
                            cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        log.LogError($"[EnsureWebSocketIsClosed] Unexpected error: {ex}");
                    }
                }
            }
        }

        void EnsureUdpSocketIsClosed()
        {
            if (udpSocket != null)
            {
                udpSocket.OnIPDiscovered -= UdpSocket_OnIPDiscovered;
                udpSocket.OnClosedPrematurely -= UdpSocket_OnClosedPrematurely;

                if (udpSocket.IsConnected)
                    udpSocket.Shutdown();
            }
        }

        async Task EnsureUserLeftVoiceChannel(CancellationToken cancellationToken)
        {
            try
            {
                await gateway.SendVoiceStateUpdatePayload(Guild.Id, null, false, false, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Gateway was disconnected while sending the payload, at this point
                // the user will automatically leave.
            }
        }

        void Invalidate()
        {
            if (isValid)
            {
                isValid = false;
                isConnecting = false;
                isConnected = false;
                connectingCancellationSource?.Cancel();

                log.LogVerbose("[Invalidate] Invalidating voice connection...");

                Shard.Voice.RemoveVoiceConnection(Guild.Id);

                OnInvalidated?.Invoke(this, new VoiceConnectionEventArgs(Shard, this));
            }
        }

        /// <summary>
        /// Invalidates the connection and releases all resources used by this voice connection.
        /// </summary>
        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                connectingCancellationSource?.Dispose();

                Invalidate();

                webSocket?.Dispose();
                udpSocket?.Dispose();
            }
        }
    }
}
