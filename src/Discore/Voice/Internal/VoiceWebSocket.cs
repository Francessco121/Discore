using Discore.WebSocket;
using Discore.WebSocket.Internal;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Voice.Internal
{
    partial class VoiceWebSocket : DiscordClientWebSocket, IDisposable
    {
        /// <summary>
        /// Called when the socket is closed unexpectedly (meaing our side did not initiate it).
        /// </summary>
        public event EventHandler? OnUnexpectedClose;
        /// <summary>
        /// Called when the socket is still connected but the heartbeat loop timed out.
        /// </summary>
        public event EventHandler? OnTimedOut;
        /// <summary>
        /// Called when the speaking state of another user in the voice channel changes.
        /// </summary>
        public event EventHandler<VoiceSpeakingEventArgs>? OnUserSpeaking;
        /// <summary>
        /// Called when the socket encountered an event requiring a new session.
        /// </summary>
        public event EventHandler? OnNewSessionRequested;
        /// <summary>
        /// Called when the socket encountered an event requiring a resume.
        /// </summary>
        public event EventHandler? OnResumeRequested;

        public const int GATEWAY_VERSION = 4;

        CancellationTokenSource? heartbeatCancellationSource;

        readonly DiscoreLogger log;

        bool isDisposed;

        bool receivedHeartbeatAck;
        uint heartbeatNonce;

        readonly Dictionary<VoiceOPCode, PayloadCallback> payloadHandlers;

        public VoiceWebSocket(string loggingName) 
            : base(loggingName)
        {
            log = new DiscoreLogger(loggingName);

            payloadHandlers = InitializePayloadHandlers();
        }

        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="WebSocketException">Thrown if the socket is not in a valid state to be closed.</exception>
        public override async Task DisconnectAsync(WebSocketCloseStatus closeStatus, string statusDescription, 
            CancellationToken cancellationToken)
        {
            // Disconnect the socket
            await base.DisconnectAsync(closeStatus, statusDescription, cancellationToken)
                .ConfigureAwait(false);

            // Cancel the heartbeat loop if it hasn't ended already
            heartbeatCancellationSource?.Cancel();
        }

        protected override void OnCloseReceived(WebSocketCloseStatus closeStatus, string? closeDescription)
        {
            if (closeStatus == WebSocketCloseStatus.NormalClosure)
                return;

            var voiceCloseCode = (VoiceCloseCode)closeStatus;
            switch (voiceCloseCode)
            {
                case VoiceCloseCode.Disconnected:
                    // Kicked or channel was deleted, don't reconnect
                    break;
                case VoiceCloseCode.VoiceServerCrashed:
                    heartbeatCancellationSource?.Cancel();
                    OnResumeRequested?.Invoke(this, EventArgs.Empty);
                    break;
                case VoiceCloseCode.InvalidSession:
                case VoiceCloseCode.SessionTimeout:
                    heartbeatCancellationSource?.Cancel();
                    OnNewSessionRequested?.Invoke(this, EventArgs.Empty);
                    break;
                default:
                    if ((int)voiceCloseCode >= 4000)
                        log.LogVerbose($"Fatal close code: {voiceCloseCode} ({(int)voiceCloseCode}), {closeDescription}");
                    else
                        log.LogVerbose($"Fatal close code: {closeStatus} ({(int)closeStatus}), {closeDescription}");

                    OnUnexpectedClose?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        protected override void OnClosedPrematurely()
        {
            OnUnexpectedClose?.Invoke(this, EventArgs.Empty);
        }

        protected override Task OnPayloadReceived(JsonDocument payload)
        {
            JsonElement payloadRoot = payload.RootElement;

            var op = (VoiceOPCode)payloadRoot.GetProperty("op").GetInt32();
            JsonElement d = payloadRoot.GetProperty("d");

            PayloadCallback? callback;
            if (payloadHandlers.TryGetValue(op, out callback))
                callback(payloadRoot, d);
            else
                log.LogWarning($"Missing handler for payload: {op} ({(int)op})");

            return Task.CompletedTask;
        }

        public async Task HeartbeatLoop(int heartbeatInterval)
        {
            receivedHeartbeatAck = true;

            while (State == WebSocketState.Open && !heartbeatCancellationSource!.IsCancellationRequested)
            {
                if (!receivedHeartbeatAck)
                {
                    log.LogWarning("[HeartbeatLoop] Connection timed out (did not receive ack for last heartbeat).");

                    OnTimedOut?.Invoke(this, EventArgs.Empty);
                    break;
                }

                try
                {
                    receivedHeartbeatAck = false;

                    // Send heartbeat
                    await SendHeartbeatPayload().ConfigureAwait(false);
                }
                catch (InvalidOperationException)
                {
                    // Socket was closed between the loop check and sending the heartbeat
                    break;
                }
                catch (DiscordWebSocketException dwex)
                {
                    // Expected to be the socket closing while sending a heartbeat
                    if (dwex.Error != DiscordWebSocketError.ConnectionClosed)
                        // Unexpected errors may not be the socket closing/aborting, so just log and loop around.
                        log.LogError($"[HeartbeatLoop] Unexpected error occured while sending a heartbeat: {dwex}");
                    else
                        break;
                }

                try
                {
                    // Wait heartbeat interval
                    await Task.Delay(heartbeatInterval, heartbeatCancellationSource.Token)
                        .ConfigureAwait(false);
                }
                catch (ObjectDisposedException)
                {
                    // VoiceWebSocket was disposed between sending a heartbeat payload and beginning to wait
                    break;
                }
                catch (OperationCanceledException)
                {
                    // Socket is disconnecting
                    break;
                }
            }
        }

        public override void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                heartbeatCancellationSource?.Dispose();
                base.Dispose();
            }
        }
    }
}
