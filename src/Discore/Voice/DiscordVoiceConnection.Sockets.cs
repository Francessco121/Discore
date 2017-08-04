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
    partial class DiscordVoiceConnection
    {
        internal async Task OnVoiceStateUpdated(DiscordVoiceState voiceState)
        {
            if (isValid)
            {
                this.voiceState = voiceState;

                if (!isConnected && !isConnecting && token != null && endPoint != null)
                    // Either the token or session ID can be received first,
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

                    // Either the token or session ID can be received first,
                    // so we must check if we are ready to start in both cases.
                    await ConnectWebSocket().ConfigureAwait(false);
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
                webSocket.OnReady -= WebSocket_OnReady;
                webSocket.OnSessionDescription -= WebSocket_OnSessionDescription;
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
                if (ex is SocketException socketEx)
                    log.LogError($"[OnReady] Failed to connect UDP socket: code = {socketEx.SocketErrorCode}, error = {ex}");
                else
                    log.LogError($"[OnReady] Failed to connect UDP socket: {ex}");

                try
                {
                    await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "An internal client error occured.",
                        VoiceConnectionInvalidationReason.Error, "Failed to connect the UDP socket.")
                        .ConfigureAwait(false);
                }
                catch (Exception closeEx)
                {
                    log.LogError($"[WebSocket_OnReady:UDPConnect:CloseAndInvalidate] {closeEx}");
                }

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
                if (ex is SocketException socketEx)
                    log.LogError($"[OnReady] Failed start IP discovery: code = {socketEx.SocketErrorCode}, error = {ex}");
                else
                    log.LogError($"[OnReady] Failed start IP discovery: {ex}");
                try
                {
                    await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "An internal client error occured.",
                        VoiceConnectionInvalidationReason.Error, "Failed to start IP discovery.")
                        .ConfigureAwait(false);
                }
                catch (Exception closeEx)
                {
                    log.LogError($"[WebSocket_OnReady:IPDiscovery:CloseAndInvalidate] {closeEx}");
                }

                return;
            }
        }

        private async void UdpSocket_OnIPDiscovered(object sender, IPDiscoveryEventArgs e)
        {
            if (!isValid || !isConnected)
                return;

            log.LogVerbose($"[IPDiscovery] Discovered EndPoint: {e.IP}:{e.Port}");

            try
            {
                // Select protocol
                await webSocket.SendSelectProtocolPayload(e.IP, e.Port, "xsalsa20_poly1305").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is DiscordWebSocketException dwex)
                    log.LogError($"[OnIPDiscovered] Failed to select protocol: code = {dwex.Error}, error = {ex}");
                else
                    log.LogError($"[OnIPDiscovered] Failed to select protocol: {ex}");

                try
                {
                    await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "An internal client error occured.",
                        VoiceConnectionInvalidationReason.Error, "Failed to select voice protocol.")
                        .ConfigureAwait(false);
                }
                catch (Exception closeEx)
                {
                    log.LogError($"[UdpSocket_OnIPDiscovered:CloseAndInvalidate] {closeEx}");
                }
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
            udpSocket = new VoiceUdpSocket($"VoiceUDPSocket:{guildId}");
            udpSocket.OnIPDiscovered += UdpSocket_OnIPDiscovered;
            udpSocket.OnClosedPrematurely += UdpSocket_OnClosedPrematurely;

            // Connect UDP socket
            await udpSocket.ConnectAsync(endPoint, port).ConfigureAwait(false);
        }

        async Task ConnectWebSocket()
        {
            // Create WebSocket
            webSocket = new VoiceWebSocket($"VoiceWebSocket:{guildId}");
            webSocket.OnReady += WebSocket_OnReady;
            webSocket.OnSessionDescription += WebSocket_OnSessionDescription;
            webSocket.OnUnexpectedClose += WebSocket_OnUnexpectedClose;
            webSocket.OnTimedOut += WebSocket_OnTimedOut;
            webSocket.OnUserSpeaking += WebSocket_OnUserSpeaking;

            // Build WebSocket URI
            Uri uri = new Uri($"wss://{endPoint}?v={GATEWAY_VERSION}");

            log.LogVerbose($"Connecting WebSocket to {uri}...");

            try
            {
                // Connect WebSocket
                await webSocket.ConnectAsync(uri, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is WebSocketException wsex)
                    log.LogError($"Failed to connect to {uri}: code = {wsex.WebSocketErrorCode}, error = {wsex}");
                else
                    log.LogError($"Failed to connect to {uri}: {ex}");

                await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "An internal client error occured.",
                    VoiceConnectionInvalidationReason.Error, "Failed to connect WebSocket.")
                    .ConfigureAwait(false);

                return;
            }

            log.LogVerbose("Connected WebSocket.");

            try
            {
                // Send IDENTIFY payload
                await webSocket.SendIdentifyPayload(guildId, Shard.UserId.Value, voiceState.SessionId, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is DiscordWebSocketException dwex)
                    log.LogError($"[ConnectSocket] Failed to send identify payload: code = {dwex.Error}, error = {ex}");
                else
                    log.LogError($"[ConnectSocket] Failed to send identify payload: {ex}");

                await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "An internal client error occured.",
                    VoiceConnectionInvalidationReason.Error, "Failed to send IDENTIFY.")
                    .ConfigureAwait(false);

                return;
            }

            // We are finished connecting
            isConnected = true;
            isConnecting = false;
            connectingCancellationSource.Cancel();

            // Ensure speaking is set
            if (isSpeaking)
            {
                try
                {
                    // Set initial speaking state
                    await SetSpeakingAsync(isSpeaking).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (ex is DiscordWebSocketException dwex)
                        log.LogError($"[ConnectSocket] Failed to set initial speaking state: code = {dwex.Error}, error = {dwex}");
                    else
                        log.LogError($"[ConnectSocket] Failed to set initial speaking state: {ex}");

                    await CloseAndInvalidate(WebSocketCloseStatus.NormalClosure, "An internal client error occured.",
                        VoiceConnectionInvalidationReason.Error, "Failed to set initial speaking state.")
                        .ConfigureAwait(false);

                    return;
                }
            }

            OnConnected?.Invoke(this, new VoiceConnectionEventArgs(Shard, this));
        }
    }
}
