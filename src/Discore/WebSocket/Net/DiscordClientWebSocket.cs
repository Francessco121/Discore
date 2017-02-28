using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    abstract class DiscordClientWebSocket : IDisposable
    {
        const int SEND_BUFFER_SIZE = 4 * 1024;  // 4kb (Discord's max payload size)
        const int RECEIVE_BUFFER_SIZE = 12 * 1024; // 12kb

        ClientWebSocket socket;

        CancellationTokenSource abortCancellationSource;
        Task sendTask;
        Task receiveTask;

        DiscoreLogger log;

        protected DiscordClientWebSocket(DiscoreLogger log)
        {
            this.log = log;

            socket = new ClientWebSocket();
            socket.Options.KeepAliveInterval = TimeSpan.Zero;
        }

        protected abstract void OnPayloadReceived(DiscordApiData payload);
        protected abstract void OnCloseReceived(WebSocketCloseStatus closeStatus, string closeDescription);
        protected abstract void OnError(WebSocketError error, string message, int nativeErrorCode);

        /// <param name="cancellationToken">Token that when cancelled will abort the entire socket.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="uri"/> does not start with ws:// or wss://.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="uri"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the socket attempts to start after a first time. A WebSocket instance
        /// can only be used for one connection attempt.
        /// </exception>
        /// <exception cref="TaskCanceledException"></exception>
        /// <exception cref="ObjectDisposedException">Thrown if this socket has already been disposed.</exception>
        /// <exception cref="WebSocketException">Thrown if the socket fails to connect.</exception>
        public virtual async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            // Attempt to connect
            log.LogVerbose($"Connecting to {uri}...");
            await socket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);

            // Shouldn't ever happen, but just in case
            if (socket.State != WebSocketState.Open)
            {
                log.LogError($"Socket.ConnectAsync succeeded but the state is {socket.State}!");
                throw new WebSocketException(WebSocketError.Faulted, "Failed to connect. No other information is available.");
            }

            // Connection successful (exception would be thrown otherwise)
            abortCancellationSource = new CancellationTokenSource();

            // Start send and receive tasks
            sendTask = SendLoop();
            receiveTask = ReceiveLoop();

            log.LogVerbose("Successfully connected.");
        }

        /// <param name="cancellationToken">Token that when cancelled will abort the entire socket.</param>
        /// <exception cref="TaskCanceledException"></exception>
        /// <exception cref="WebSocketException">Thrown if the socket is not in a valid state to be closed.</exception>
        public virtual Task DisconnectAsync(WebSocketCloseStatus closeStatus, string statusDescription,
            CancellationToken cancellationToken)
        {
            // Since this operation can take up to 5s and is synchronous code, wrap it in a task.
            return Task.Run(() =>
            {
                log.LogVerbose($"Disconnecting with code {closeStatus} ({(int)closeStatus})...");

                // Since the state check will be eaten by the code below,
                // and we cannot reliably check for this below, check if
                // the socket state is valid for a close, and throw the
                // same exception CloseAsync would have if not valid.
                if (socket.State != WebSocketState.Open && socket.State != WebSocketState.CloseReceived
                    && socket.State != WebSocketState.CloseSent)
                    throw new WebSocketException(WebSocketError.InvalidState, $"Socket is in an invalid state: {socket.State}");

                // CloseAsync will send the close message and then wait until
                // a close frame has been received. Once nothing is holding
                // the receive lock, CloseAsync will then try and receive
                // data until it comes to a close frame, or another receive
                // call gets it.
                Task closeTask = socket.CloseAsync(closeStatus, statusDescription, cancellationToken);

                try
                {
                    // Give the socket 5s to gracefully disconnect.
                    if (!Task.WaitAll(new Task[] { closeTask, sendTask, receiveTask }, 5000, cancellationToken))
                    {
                        // Socket did not gracefully disconnect in the given time,
                        // so abort the socket and move on.
                        log.LogWarning($"Socket failed to disconnect after 5s, aborting...");
                        abortCancellationSource.Cancel();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Caller cancelled, socket has been aborted.
                    log.LogVerbose("Disconnect cancelled by caller, socket has been aborted.");
                    throw;
                }
                catch (AggregateException aex)
                {
                    foreach (Exception ex in aex.InnerExceptions)
                    {
                        if (ex is OperationCanceledException)
                            // Cancellations are normal.
                            continue;

                        WebSocketException wsex = ex as WebSocketException;
                        if (wsex != null && wsex.WebSocketErrorCode == WebSocketError.InvalidState)
                            // The socket being aborted is normal.
                            continue;

                        log.LogError($"Uncaught exception found while disconnecting: {ex}");
                    }
                }

                log.LogVerbose("Disconnected.");
            });
        }

        async Task SendLoop()
        {

        }

        async Task ReceiveLoop()
        {

        }

        public virtual void Dispose()
        {
            abortCancellationSource?.Dispose();
            socket.Dispose();
        }
    }
}
