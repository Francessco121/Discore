using Discore.Voice.Net;
using Discore.WebSocket;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Discore.Voice
{
    partial class DiscordVoiceConnection
    {
        VoiceWebSocket webSocket;
        string endPoint;
        string token;
        int? heartbeatInterval;
        int? port;
        int? ssrc;
        string[] encryptionModes;
        Task heartbeatLoopTask;
        VoiceUdpSocket udpSocket;
        string discoveredIP;
        int? discoveredPort;

        internal async Task OnVoiceStateUpdated(DiscordVoiceState voiceState)
        {
            if (isValid)
            {
                this.voiceState = voiceState;

                if (!isConnected && !isConnecting && token != null && endPoint != null)
                    // Either the token or session ID can be received first,
                    // so we must check if we are ready to start in both cases.
                    //await ConnectWebSocket().ConfigureAwait(false);
                    await DoFullConnect();
            }
        }

        internal async Task OnVoiceServerUpdated(string token, string endPoint)
        {
            if (isValid)
            {
                this.token = token;
                // Strip off the port
                this.endPoint = endPoint.Split(':')[0];

                if (voiceState != null)
                {
                    // Server updates can be sent twice, the second time
                    // is when the voice server changes, so we need to reconnect.
                    if (isConnected)
                    {
                        await EnsureWebSocketIsClosed(WebSocketCloseStatus.NormalClosure, "Reconnecting...").ConfigureAwait(false);
                        EnsureUdpSocketIsClosed();
                    }

                    // Either the token or session ID can be received first,
                    // so we must check if we are ready to start in both cases.
                    //await ConnectWebSocket().ConfigureAwait(false);
                    await DoFullConnect();
                }
            }
        }

        async Task DoFullConnect()
        {
            try
            {
                Func<Task>[] functions = new Func<Task>[]
                {
                    CreateVoiceWebSocket,
                    ConnectVoiceWebSocket,
                    ReceiveVoiceHello,
                    SendVoiceIdentify,
                    ReceiveVoiceReady,
                    BeginHeartbeatLoop,
                    CreateVoiceUdpSocket,
                    ConnectVoiceUdpSocket,
                    StartIPDiscovery,
                    ReceiveIPDiscovery,
                    SendSelectProtocol,
                    ReceiveSessionDescription
                };

                foreach (Func<Task> function in functions)
                {
                    if (!isValid)
                        throw new TaskCanceledException("Connection was invalidated before completion.");

                    await function();
                }

                isConnected = true;
                isConnecting = false;
                connectingCancellationSource.Cancel();

                OnConnected?.Invoke(this, new VoiceConnectionEventArgs(shard, this));
            }
            catch (Exception ex)
            {
                log.LogError($"[DoFullConnect] Failed to connect: {ex}");

                try
                {
                    await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "An internal client error occured.",
                        VoiceConnectionInvalidationReason.Error, "Failed to connect.")
                        .ConfigureAwait(false);
                }
                catch (Exception closeEx)
                {
                    log.LogError($"[DoFullConnect:CloseAndInvalidate] {closeEx}");
                }
            }
        }

        /// <summary>
        /// Ensures that: the WebSocket is closed, the UDP socket is closed, the user has left the voice channel,
        /// and the connection is invalidated.
        /// </summary>
        /// <exception cref="OperationCanceledException"></exception>
        async Task CloseAndInvalidate(WebSocketCloseStatus webSocketCloseCode, string webSocketCloseDescription,
            VoiceConnectionInvalidationReason reason, string errorMessage = null,
            CancellationToken? cancellationToken = null)
        {
            Task leaveChannelTask = EnsureUserLeftVoiceChannel(cancellationToken ?? CancellationToken.None);

            Task webSocketDisconnectTask = EnsureWebSocketIsClosed(webSocketCloseCode,
                webSocketCloseDescription, cancellationToken);

            EnsureUdpSocketIsClosed();

            await webSocketDisconnectTask.ConfigureAwait(false);
            await leaveChannelTask.ConfigureAwait(false);

            Invalidate(reason, errorMessage);
        }

        void Invalidate(VoiceConnectionInvalidationReason reason, string errorMessage = null)
        {
            if (isValid)
            {
                isValid = false;
                isConnecting = false;
                isConnected = false;

                voiceState = null;

                if (!isDisposed)
                    connectingCancellationSource?.Cancel();

                log?.LogVerbose("[Invalidate] Invalidating voice connection...");

                Shard.Voice.RemoveVoiceConnection(guildId);

                OnInvalidated?.Invoke(this, new VoiceConnectionInvalidatedEventArgs(Shard, this, reason, errorMessage));
            }
        }

        /// <exception cref="OperationCanceledException"></exception>
        async Task EnsureWebSocketIsClosed(WebSocketCloseStatus webSocketCloseStatus, string webSocketCloseDescription,
            CancellationToken? cancellationToken = null)
        {
            isConnected = false;

            if (webSocket != null)
            {
                webSocket.OnUnexpectedClose -= WebSocket_OnUnexpectedClose;
                webSocket.OnTimedOut -= WebSocket_OnTimedOut;
                webSocket.OnUserSpeaking -= WebSocket_OnUserSpeaking;

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
                udpSocket.OnClosedPrematurely -= UdpSocket_OnClosedPrematurely;

                if (udpSocket.IsConnected)
                    udpSocket.Shutdown();
            }
        }

        async Task EnsureUserLeftVoiceChannel(CancellationToken cancellationToken)
        {
            try
            {
                await gateway.SendVoiceStateUpdatePayload(guildId, null, false, false, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException oex)
            {
                if (oex.CancellationToken != cancellationToken)
                {
                    // Gateway was disconnected while sending the payload, at this point
                    // the user will automatically leave.
                }
                else
                    throw;
            }
        }

        private void WebSocket_OnUserSpeaking(object sender, VoiceSpeakingEventArgs e)
        {
            OnMemberSpeaking?.Invoke(this, new MemberSpeakingEventArgs(guildId, e.UserId, e.IsSpeaking, Shard, this));
        }

        private async void UdpSocket_OnClosedPrematurely(object sender, EventArgs e)
        {
            try
            {
                await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "An internal client error occured.",
                    VoiceConnectionInvalidationReason.Error, "The UDP connection closed unexpectedly.")
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogError($"[UdpSocket_OnClosedPrematurely] {ex}");
            }
        }

        private async void WebSocket_OnUnexpectedClose(object sender, EventArgs e)
        {
            try
            {
                await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "An internal client error occured.",
                    VoiceConnectionInvalidationReason.Error, "The WebSocket connection closed unexpectedly.")
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogError($"[WebSocket_OnUnexpectedClose] {ex}");
            }
        }

        private async void WebSocket_OnTimedOut(object sender, EventArgs e)
        {
            try
            {
                await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "Connection timed out.",
                    VoiceConnectionInvalidationReason.TimedOut, "The WebSocket connection timed out.")
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogError($"[WebSocket_OnTimedOut] {ex}");
            }
        }

        Task CreateVoiceWebSocket()
        {
            if (webSocket != null)
                throw new InvalidOperationException("[CreateVoiceWebSocket] webSocket must be null!");
            if (guildId == Snowflake.None)
                throw new InvalidOperationException("[CreateVoiceWebSocket] guildId must be set!");

            webSocket = new VoiceWebSocket($"VoiceWebSocket:{guildId}");
            webSocket.OnUnexpectedClose += WebSocket_OnUnexpectedClose;
            webSocket.OnTimedOut += WebSocket_OnTimedOut;
            webSocket.OnUserSpeaking += WebSocket_OnUserSpeaking;

            log.LogVerbose("[CreateVoiceWebSocket] Created VoiceWebSocket.");

            return Task.CompletedTask;
        }

        async Task ConnectVoiceWebSocket()
        {
            if (webSocket == null)
                throw new InvalidOperationException("[ConnectVoiceWebSocket] webSocket must not be null!");
            if (endPoint == null)
                throw new InvalidOperationException("[ConnectVoiceWebSocket] endPoint must not be null!");

            // Build WebSocket URI
            Uri uri = new Uri($"wss://{endPoint}?v={VoiceWebSocket.GATEWAY_VERSION}");

            log.LogVerbose($"[ConnectVoiceWebSocket] Connecting WebSocket to {uri}...");

            // Connect
            try
            {
                await webSocket.ConnectAsync(uri, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (WebSocketException ex)
            {
                log.LogError($"[ConnectVoiceWebSocket] Failed to connect to {uri}: " +
                    $"code = {ex.WebSocketErrorCode}, error = {ex}");
                throw;
            }

            log.LogVerbose("[ConnectVoiceWebSocket] Connected WebSocket.");
        }

        Task ReceiveVoiceHello()
        {
            if (webSocket == null)
                throw new InvalidOperationException("[ReceiveVoiceHello] webSocket must not be null!");

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(10 * 1000);

            return Task.Run(() =>
            {
                heartbeatInterval = webSocket.HelloQueue.Take(tokenSource.Token);
            });
        }

        async Task SendVoiceIdentify()
        {
            if (webSocket == null)
                throw new InvalidOperationException("[SendVoiceIdentify] webSocket must not be null!");
            if (!shard.UserId.HasValue)
                throw new InvalidOperationException("[SendVoiceIdentify] shard.UserId must not be null!");
            if (voiceState == null)
                throw new InvalidOperationException("[SendVoiceIdentify] voiceState must not be null!");
            if (voiceState.SessionId == null)
                throw new InvalidOperationException("[SendVoiceIdentify] voiceState.SessionId must not be null!");
            if (token == null)
                throw new InvalidOperationException("[SendVoiceIdentify] token must not be null!");

            await webSocket.SendIdentifyPayload(guildId, shard.UserId.Value, voiceState.SessionId, token)
                .ConfigureAwait(false);
        }

        Task ReceiveVoiceReady()
        {
            if (webSocket == null)
                throw new InvalidOperationException("[ReceiveVoiceReady] webSocket must not be null!");

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(10 * 1000);

            return Task.Run(() =>
            {
                VoiceReadyEventArgs readyData = webSocket.ReadyQueue.Take(tokenSource.Token);

                log.LogVerbose($"[ReceiveVoiceReady] ssrc = {readyData.Ssrc}, port = {readyData.Port}");

                encryptionModes = readyData.EncryptionModes;
                port = readyData.Port;
                ssrc = readyData.Ssrc;
            });
        }

        Task BeginHeartbeatLoop()
        {
            if (webSocket == null)
                throw new InvalidOperationException("[BeginHeartbeatLoop] webSocket must not be null!");
            if (!heartbeatInterval.HasValue)
                throw new InvalidOperationException("[BeginHeartbeatLoop] heartbeatInterval must not be null!");

            heartbeatLoopTask = webSocket.HeartbeatLoop(heartbeatInterval.Value);
            return Task.CompletedTask;
        }

        Task CreateVoiceUdpSocket()
        {
            if (udpSocket != null)
                throw new InvalidOperationException("[CreateVoiceUdpSocket] udpSocket must be null!");
            if (!ssrc.HasValue)
                throw new InvalidOperationException("[CreateVoiceUdpSocket] ssrc must not be null!");

            udpSocket = new VoiceUdpSocket($"VoiceUDPSocket:{guildId}", ssrc.Value);
            udpSocket.OnClosedPrematurely += UdpSocket_OnClosedPrematurely;

            log.LogVerbose("[CreateVoiceUdpSocket] Created VoiceUdpSocket.");

            return Task.CompletedTask;
        }

        async Task ConnectVoiceUdpSocket()
        {
            if (udpSocket == null)
                throw new InvalidOperationException("[ConnectVoiceUdpSocket] udpSocket must not be null!");
            if (string.IsNullOrWhiteSpace(endPoint))
                throw new InvalidOperationException("[ConnectVoiceUdpSocket] endPoint must be set!");
            if (!port.HasValue)
                throw new InvalidOperationException("[ConnectVoiceUdpSocket] port must not be null!");

            log.LogVerbose($"[ConnectVoiceUdpSocket] Connecting UdpSocket to {endPoint}:{port}...");

            try
            {
                await udpSocket.ConnectAsync(endPoint, port.Value).ConfigureAwait(false);
            }
            catch (SocketException ex)
            {
                log.LogError("[ConnectVoiceUdpSocket] Failed to connect UDP socket: " +
                    $"code = {ex.SocketErrorCode}, error = {ex}");
                throw;
            }
        }

        async Task StartIPDiscovery()
        {
            if (udpSocket == null)
                throw new InvalidOperationException("[StartIPDiscovery] udpSocket must not be null!");

            try
            {
                await udpSocket.StartIPDiscoveryAsync().ConfigureAwait(false);
            }
            catch (SocketException ex)
            {
                log.LogError("[StartIPDiscovery] Failed start IP discovery: " +
                    $"code = {ex.SocketErrorCode}, error = {ex}");
            }
        }

        Task ReceiveIPDiscovery()
        {
            if (udpSocket == null)
                throw new InvalidOperationException("[ReceiveIPDiscovery] udpSocket must not be null!");

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(10 * 1000);

            return Task.Run(() =>
            {
                IPDiscoveryEventArgs ipData = udpSocket.IPDiscoveryQueue.Take(tokenSource.Token);

                log.LogVerbose($"[ReceiveIPDiscovery] Discovered end-point: {ipData.IP}:{ipData.Port}");

                discoveredIP = ipData.IP;
                discoveredPort = ipData.Port;
            });
        }

        async Task SendSelectProtocol()
        {
            if (webSocket == null)
                throw new InvalidOperationException("[SendSelectProtocol] webSocket must not be null!");
            if (string.IsNullOrWhiteSpace(discoveredIP))
                throw new InvalidOperationException("[SendSelectProtocol] discoveredIP must be set!");
            if (!discoveredPort.HasValue)
                throw new InvalidOperationException("[SendSelectProtocol] discoveredPort must not be null!");

            try
            {
                await webSocket.SendSelectProtocolPayload(discoveredIP, discoveredPort.Value,
                    "xsalsa20_poly1305").ConfigureAwait(false);
            }
            catch (DiscordWebSocketException ex)
            {
                log.LogError($"[OnIPDiscovered] Failed to select protocol: code = {ex.Error}, error = {ex}");
            }
        }

        Task ReceiveSessionDescription()
        {
            if (webSocket == null)
                throw new InvalidOperationException("[ReceiveSessionDescription] webSocket must not be null!");
            if (udpSocket == null)
                throw new InvalidOperationException("[ReceiveSessionDescription] udpSocket must not be null!");

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(10 * 1000);

            return Task.Run(() =>
            {
                VoiceSessionDescriptionEventArgs sessionDescription =
                    webSocket.SessionDescriptionQueue.Take(tokenSource.Token);

                Debug.Assert(sessionDescription.Mode == "xsalsa20_poly1305");

                udpSocket.Start(sessionDescription.SecretKey);
            });
        }
    }
}
