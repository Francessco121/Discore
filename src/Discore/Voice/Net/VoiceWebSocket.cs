using Discore.WebSocket;
using Discore.WebSocket.Net;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Voice.Net
{
    partial class VoiceWebSocket : DiscordClientWebSocket, IDisposable
    {
        /// <summary>
        /// Called when the socket receives the READY payload.
        /// </summary>
        public event EventHandler<VoiceReadyEventArgs> OnReady;
        /// <summary>
        /// Called when the socket receives the SESSION_DESCRIPTION payload.
        /// </summary>
        public event EventHandler<VoiceSessionDescriptionEventArgs> OnSessionDescription;
        /// <summary>
        /// Called when the socket is closed unexpectedly (meaing our side did not initiate it).
        /// </summary>
        public event EventHandler OnUnexpectedClose;
        /// <summary>
        /// Called when the socket is still connected but the heartbeat loop timed out.
        /// </summary>
        public event EventHandler OnTimedOut;

        Task heartbeatTask;
        CancellationTokenSource heartbeatCancellationSource;

        DiscoreLogger log;

        bool isDisposed;

        int heartbeatInterval;
        bool receivedHeartbeatAck;

        public VoiceWebSocket(string loggingName) 
            : base(loggingName)
        {
            log = new DiscoreLogger(loggingName);

            InitializePayloadHandlers();
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

        protected override void OnCloseReceived(WebSocketCloseStatus closeStatus, string closeDescription)
        {
            // TODO: may need to do some extra work depending on the close status,
            // but none are documented...

            OnUnexpectedClose?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnClosedPrematurely()
        {
            OnUnexpectedClose?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPayloadReceived(DiscordApiData payload)
        {
            VoiceOPCode op = (VoiceOPCode)payload.GetInteger("op").Value;
            DiscordApiData d = payload.Get("d");

            PayloadCallback callback;
            if (payloadHandlers.TryGetValue(op, out callback))
                callback(payload, d);
            else
                log.LogWarning($"Missing handler for payload: {op} ({(int)op})");
        }

        async Task HeartbeatLoop()
        {
            receivedHeartbeatAck = true;

            while (State == WebSocketState.Open && !heartbeatCancellationSource.IsCancellationRequested)
            {
                if (!receivedHeartbeatAck)
                {
                    log.LogVerbose("[HeartbeatLoop] Connection timed out.");

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

                heartbeatCancellationSource.Dispose();
                base.Dispose();
            }
        }
    }
}
