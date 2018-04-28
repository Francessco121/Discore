using Discore.Voice.Net;
using Discore.WebSocket;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Voice
{
    partial class DiscordVoiceConnection
    {
        VoiceWebSocket webSocket;
        string endPoint;
        string token;
        int? heartbeatInterval;
        IPAddress udpIP;
        int? udpPort;
        int? ssrc;
        string[] encryptionModes;
        Task heartbeatLoopTask;
        VoiceUdpSocket udpSocket;
        string discoveredIP;
        int? discoveredPort;

        Task resumeTask;
        CancellationTokenSource resumeCancellationTokenSource;

        internal async Task OnVoiceStateUpdated(DiscordVoiceState voiceState)
        {
            if (isValid)
            {
                this.voiceState = voiceState;

                if (!isConnected && !isConnecting && token != null && endPoint != null)
                    // Either the token or session ID can be received first,
                    // so we must check if we are ready to start in both cases.
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

                // Either the token or session ID can be received first,
                // so we must check if we are ready to start in both cases.
                if (voiceState != null)
                {
                    // Server updates can be sent twice, the second time
                    // is when the voice server changes, so we need to reconnect.
                    bool isServerSwap = isConnected;

                    if (isServerSwap)
                    {
                        await EnsureWebSocketIsClosed(WebSocketCloseStatus.NormalClosure, "Reconnecting...").ConfigureAwait(false);
                        EnsureUdpSocketIsClosed();
                    }
                    
                    // Start a new session
                    await DoFullConnect();

                    if (isServerSwap)
                    {
                        // Ensure we send a speaking payload so that moving between voice servers
                        // updates the ssrc correctly.
                        await webSocket.SendSpeakingPayload(isSpeaking, ssrc.Value)
                            .ConfigureAwait(false);
                    }
                }
            }
        }

        async Task DoFullConnect()
        {
            isConnecting = true;

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

                if (!isConnected)
                {
                    isConnected = true;
                    isConnecting = false;
                    connectingCancellationSource.Cancel();

                    OnConnected?.Invoke(this, new VoiceConnectionEventArgs(shard, this));
                }
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
            finally
            {
                if (isConnected)
                    // If this isn't the first full connect, we just need
                    // to make sure isConnecting goes back to false.
                    isConnecting = false;
            }
        }

        async Task DoResume()
        {
            isConnecting = true;
            resumeCancellationTokenSource = new CancellationTokenSource();

            try
            {
                Func<Task>[] functions = new Func<Task>[]
                {
                    CreateVoiceWebSocket,
                    ConnectVoiceWebSocket,
                    SendVoiceResume,
                    ReceiveResumed,
                    ReceiveVoiceHello,
                    BeginHeartbeatLoop,
                };

                foreach (Func<Task> function in functions)
                {
                    if (!isValid || resumeCancellationTokenSource.IsCancellationRequested)
                        throw new TaskCanceledException("Connection was invalidated before completion.");

                    await function();
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException && resumeCancellationTokenSource.IsCancellationRequested)
                {
                    // If the resume was cancelled, this means a new session is occurring as a follow-up.
                    // In this case we do not want to close the sockets.
                    log.LogVerbose("[DoResume] Successfully cancelled.");
                    return;
                }

                log.LogError($"[DoResume] Failed to connect: {ex}");

                try
                {
                    await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "An internal client error occured.",
                        VoiceConnectionInvalidationReason.Error, "Failed to connect.")
                        .ConfigureAwait(false);
                }
                catch (Exception closeEx)
                {
                    log.LogError($"[DoResume:CloseAndInvalidate] {closeEx}");
                }
            }
            finally
            {
                // Don't flip to false if this was cancelled in favor of a full-connect.
                if (!resumeCancellationTokenSource.IsCancellationRequested)
                    isConnecting = false;
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

        private async void WebSocket_OnNewSessionRequested(object sender, EventArgs e)
        {
            if (isConnected)
            {
                log.LogVerbose("Attempting new session...");

                // Ensure sockets are disconnected
                if (webSocket != null && webSocket.CanBeDisconnected)
                    await webSocket.DisconnectAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None)
                        .ConfigureAwait(false);

                if (udpSocket != null && udpSocket.IsConnected)
                    udpSocket.Shutdown();

                if (heartbeatLoopTask != null)
                {
                    try
                    {
                        // The task is cancelled, but we need to make sure
                        // it's completely finished before continuing.
                        await heartbeatLoopTask;
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Uncaught exception in heartbeat loop: {ex}");
                    }
                }

                // Finish resume if one was in progress
                resumeCancellationTokenSource?.Cancel();
                if (resumeTask != null && !resumeTask.IsCompleted)
                    await resumeTask.ConfigureAwait(false);

                await DoFullConnect().ConfigureAwait(false);
            }
            else
            {
                log.LogVerbose("Received new session request before initial connect. Invalidating...");

                try
                {
                    await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "An internal client error occured.",
                        VoiceConnectionInvalidationReason.Error, "The WebSocket connection closed unexpectedly.")
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    log.LogError($"[WebSocket_OnNewSessionRequested] {ex}");
                }
            }
        }

        private async void WebSocket_OnResumeRequested(object sender, EventArgs e)
        {
            if (isConnected)
            {
                log.LogVerbose("Attempting resume...");

                if (webSocket != null && webSocket.CanBeDisconnected)
                    await webSocket.DisconnectAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None)
                        .ConfigureAwait(false);

                if (heartbeatLoopTask != null)
                {
                    try
                    {
                        // The task is cancelled, but we need to make sure
                        // it's completely finished before continuing.
                        await heartbeatLoopTask.ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Uncaught exception in heartbeat loop: {ex}");
                    }
                }

                resumeTask = DoResume();
                await resumeTask.ConfigureAwait(false);
            }
            else
            {
                log.LogVerbose("Received resume request before initial connect. Invalidating...");

                try
                {
                    await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "An internal client error occured.",
                        VoiceConnectionInvalidationReason.Error, "The WebSocket connection closed unexpectedly.")
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    log.LogError($"[WebSocket_OnResumeRequested] {ex}");
                }
            }
        }

        Task CreateVoiceWebSocket()
        {
            if (webSocket != null && webSocket.IsConnected)
                throw new InvalidOperationException("[CreateVoiceWebSocket] webSocket must be null or disconnected!");
            if (guildId == Snowflake.None)
                throw new InvalidOperationException("[CreateVoiceWebSocket] guildId must be set!");

            webSocket = new VoiceWebSocket($"VoiceWebSocket:{guildId}");
            webSocket.OnUnexpectedClose += WebSocket_OnUnexpectedClose;
            webSocket.OnTimedOut += WebSocket_OnTimedOut;
            webSocket.OnUserSpeaking += WebSocket_OnUserSpeaking;
            webSocket.OnNewSessionRequested += WebSocket_OnNewSessionRequested;
            webSocket.OnResumeRequested += WebSocket_OnResumeRequested;

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

        async Task SendVoiceResume()
        {
            if (webSocket == null)
                throw new InvalidOperationException("[SendVoiceIdentify] webSocket must not be null!");
            if (voiceState == null)
                throw new InvalidOperationException("[SendVoiceIdentify] voiceState must not be null!");
            if (voiceState.SessionId == null)
                throw new InvalidOperationException("[SendVoiceIdentify] voiceState.SessionId must not be null!");
            if (token == null)
                throw new InvalidOperationException("[SendVoiceIdentify] token must not be null!");

            await webSocket.SendResumePayload(guildId, voiceState.SessionId, token)
                .ConfigureAwait(false);
        }

        Task ReceiveResumed()
        {
            if (webSocket == null)
                throw new InvalidOperationException("[ReceiveResumed] webSocket must not be null!");

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(5 * 1000);

            return Task.Run(() =>
            {
                webSocket.ResumedQueue.Take(tokenSource.Token);
                log.LogVerbose("[ReceiveResumed] Resume successful!");
            });
        }

        Task ReceiveVoiceHello()
        {
            if (webSocket == null)
                throw new InvalidOperationException("[ReceiveVoiceHello] webSocket must not be null!");

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(5 * 1000);

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
            tokenSource.CancelAfter(5 * 1000);

            return Task.Run(() =>
            {
                VoiceReadyEventArgs readyData = webSocket.ReadyQueue.Take(tokenSource.Token);

                log.LogVerbose($"[ReceiveVoiceReady] ssrc = {readyData.Ssrc}, ip = {readyData.IP}, port = {readyData.Port}");

                udpIP = readyData.IP;
                udpPort = readyData.Port;
                ssrc = readyData.Ssrc;
                encryptionModes = readyData.EncryptionModes;
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
            if (udpSocket != null && udpSocket.IsConnected)
                throw new InvalidOperationException("[CreateVoiceUdpSocket] udpSocket must be null or disconnected!");
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
            if (udpIP == null)
                throw new InvalidOperationException("[ConnectVoiceUdpSocket] udpIP must not be null!");
            if (!udpPort.HasValue)
                throw new InvalidOperationException("[ConnectVoiceUdpSocket] udpPort must not be null!");

            log.LogVerbose($"[ConnectVoiceUdpSocket] Connecting UdpSocket to {udpIP}:{udpPort}...");

            try
            {
                await udpSocket.ConnectAsync(udpIP, udpPort.Value).ConfigureAwait(false);
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
                log.LogError("[StartIPDiscovery] Failed to start IP discovery: " +
                    $"code = {ex.SocketErrorCode}, error = {ex}");
                throw;
            }
        }

        Task ReceiveIPDiscovery()
        {
            if (udpSocket == null)
                throw new InvalidOperationException("[ReceiveIPDiscovery] udpSocket must not be null!");

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(5 * 1000);

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
                throw;
            }
        }

        Task ReceiveSessionDescription()
        {
            if (webSocket == null)
                throw new InvalidOperationException("[ReceiveSessionDescription] webSocket must not be null!");
            if (udpSocket == null)
                throw new InvalidOperationException("[ReceiveSessionDescription] udpSocket must not be null!");

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(5 * 1000);

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
